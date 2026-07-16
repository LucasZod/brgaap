# brgaap

Chat com IA embarcada pro domínio de gestão pública. A ideia é simples: um modelo roda localmente com o **Ollama**, o usuário conversa por um chat e, quando precisa de dado externo, o modelo consulta APIs (BrasilAPI, IBGE) através de um servidor **MCP** escrito em .NET 10.

O caminho da informação é esse:

```
Usuário → chat (Next.js) → chat-api (.NET 10) → Ollama → mcp-server (.NET 10) → APIs externas
```

## Módulos

São três apps, cada um com um papel bem definido:

| Módulo                               | Stack                | O que faz                                                                             |
| ------------------------------------ | -------------------- | ------------------------------------------------------------------------------------- |
| [`apps/mcp-server`](apps/mcp-server) | Console .NET 10      | Servidor MCP (stdio) que expõe as tools de consulta a CNPJ, estados e municípios      |
| [`apps/chat-api`](apps/chat-api)     | ASP.NET Core .NET 10 | Faz a ponte entre usuário, Ollama e mcp-server, e devolve a resposta em streaming SSE |
| [`apps/web`](apps/web)               | Next.js 16, React 19 | O frontend do chat, com a resposta chegando token a token                             |

## Antes de começar

Você vai precisar de:

| Ferramenta        | Versão     | Detalhe                                 |
| ----------------- | ---------- | --------------------------------------- |
| .NET SDK          | `10.0.302` | Fixado no [`global.json`](global.json)  |
| Ollama            | atual      | Tem que estar rodando antes do chat-api |
| Modelo `qwen2.5:3b` |          | Baixado no Ollama                       |

Pra conferir se está tudo no lugar:

```bash
dotnet --version        # tem que mostrar 10.0.302
ollama --version
```

## Passo 0: subir o Ollama

Sem o Ollama no ar, o chat-api não sobe. Ele espera encontrar o serviço em `http://localhost:11434`.

```bash
# deixa rodando num terminal separado
ollama serve

# baixa o modelo, só precisa fazer na primeira vez
ollama pull qwen2.5:3b

# confere se baixou
ollama list
```

## Módulo 1: mcp-server

Esse aqui é um processo console que fala MCP por stdio (JSON-RPC 2.0). Ele não abre porta HTTP nenhuma. Na prática quem sobe ele é o chat-api, como subprocesso. Você só roda ele sozinho se quiser testar isolado.

Buildar:

```bash
dotnet build apps/mcp-server
```

Detalhe importante: o chat-api chama o mcp-server com `--no-build`, então você precisa ter buildado ele antes, senão o chat-api quebra.

Se quiser testar por conta própria, o jeito mais fácil é com o MCP Inspector:

```bash
npx @modelcontextprotocol/inspector dotnet run --project apps/mcp-server
```

As 4 tools têm que aparecer: `get_cnpj_info`, `get_states`, `get_municipalities` e `get_server_date_time`.

Dá pra testar direto pelo stdin também, se preferir:

```bash
{
  echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"t","version":"1"}}}'
  echo '{"jsonrpc":"2.0","method":"notifications/initialized"}'
  echo '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'
  sleep 3
} | dotnet run --project apps/mcp-server --no-build
```

## Módulo 2: chat-api

É o backend HTTP que orquestra a conversa. Quando ele sobe, sobe junto o mcp-server como subprocesso e já carrega as tools antes de abrir a porta.

Pra rodar, garanta duas coisas antes:

1. Ollama no ar com o qwen2.5:3b (o passo 0).
2. mcp-server já buildado (`dotnet build apps/mcp-server`).

Aí é só:

```bash
dotnet run --project apps/chat-api
```

Sobe em `http://localhost:5000`.

Pra testar rápido:

```bash
curl -X POST http://localhost:5000/chat \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"t-1","message":"Que horas são no servidor?"}' \
  --no-buffer
```

A resposta vem em streaming, em pedaços `data: ...`, e fecha com `data: [DONE]`. Quem decide se vai chamar uma tool é o próprio modelo. Por exemplo, esse aqui faz ele consultar o CNPJ:

```bash
curl -X POST http://localhost:5000/chat \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"t-1","message":"Qual a situação cadastral do CNPJ 33000167000101?"}' \
  --no-buffer
```

A configuração fica no [`apps/chat-api/appsettings.json`](apps/chat-api/appsettings.json), se quiser apontar pra outro modelo ou outra URL:

```json
{
  "Ollama": { "Url": "http://localhost:11434", "Model": "qwen2.5:3b" },
  "Mcp": { "ServerProject": "../mcp-server" }
}
```

## Módulo 3: web

O frontend, em Next.js 16 com React 19 e Tailwind v4. É uma interface de coluna única, bem enxuta, que consome o chat-api via SSE e vai montando a resposta token a token. As mensagens têm animação de entrada com `motion`.

Antes de rodar:

1. chat-api no ar em `http://localhost:5000` (módulos 1 e 2).
2. Node 20.9 ou mais novo, que é o que o Next 16 pede.

Aí:

```bash
cd apps/web
npm install        # só na primeira vez
npm run dev
```

Abre em `http://localhost:3000`. Digita uma pergunta e a resposta chega em streaming.

Algumas perguntas que respondem rápido (o porquê está lá embaixo nas limitações):

| Tipo                           | O que perguntar                                                                                                         |
| ------------------------------ | ----------------------------------------------------------------------------------------------------------------------- |
| Data e hora                    | `Que horas são no servidor?`, `Qual a data de hoje?`                                                                    |
| CNPJ                           | `Consulte o CNPJ 33000167000101`, `Qual a situação cadastral do CNPJ 00000000000191?`                                   |
| Estados                        | `Liste os estados brasileiros`, `Qual a sigla e o código IBGE de Goiás?`, `Quais estados ficam na região Centro-Oeste?` |
| Municípios de estados pequenos | `Liste os municípios de Roraima`, `Municípios do Amapá`, `Quais as cidades do Distrito Federal?`, `Municípios do Acre`  |

Só evita pedir a lista completa de município de estado grande (Goiás tem 246, São Paulo 645, Minas 853). Funcionar funciona, mas o modelo 3B rodando em CPU leva minutos pra reescrever a lista inteira. Vai de estado pequeno ou de pergunta específica, tipo "a cidade tal existe em GO?".

A URL do backend também é configurável, se precisar:

```bash
# apps/web/.env.local
NEXT_PUBLIC_CHAT_API_URL=http://localhost:5000
```

## Subindo tudo de uma vez

A ordem que funciona é essa:

```bash
# 1. Ollama, no terminal A
ollama serve

# 2. build do mcp-server, no terminal B
dotnet build apps/mcp-server

# 3. chat-api, que sobe o mcp-server sozinho, ainda no terminal B
dotnet run --project apps/chat-api

# 4. web, no terminal C
cd apps/web && npm run dev
```

E os endereços de cada um:

| Serviço  | URL                    |
| -------- | ---------------------- |
| Ollama   | http://localhost:11434 |
| chat-api | http://localhost:5000  |
| web      | http://localhost:3000  |

## Estrutura do repo

```
brgaap/
├── apps/
│   ├── mcp-server/     # servidor MCP stdio com as tools
│   ├── chat-api/       # orquestrador HTTP + cliente MCP + Ollama
│   └── web/            # frontend Next.js do chat
├── global.json         # SDK fixado em 10.0.302
├── BrGaap.slnx
└── README.md
```

## Limitações conhecidas

Vale ser honesto sobre os limites da coisa, porque tem escolhas de escopo aqui que são de propósito. O projeto roda um LLM local, o qwen2.5 de 3B, em CPU via Ollama. Cheguei nele depois de testar o llama3.2 de 3B, que vivia errando a tool e respondendo em inglês, e o qwen ficou bem mais confiável em decidir quando e qual ferramenta usar. Mesmo assim, rodar um 3B em CPU cobra um preço:

A primeira coisa é a lentidão em lista grande. Quando uma tool devolve muito item, o modelo precisa ler o JSON inteiro antes de começar a responder, e depois vai cuspindo poucos tokens por segundo, tudo em CPU. Na prática, listar os 15 municípios de Roraima leva uns 45 segundos. A lista inteira de Goiás, com 246, passa de vários minutos. Consulta menor (CNPJ, estados, data, município de estado pequeno) responde numa boa.

A segunda é a fidelidade do dado. O modelo reescreve o que a tool retornou, então de vez em quando ele escorrega, troca um acento no nome de um município ou solta o código IBGE errado (já vi dizer 53 pro Goiás, sendo que é 52). A tool devolve o dado certo. Quem escorrega é o modelo de 3B na hora de reproduzir, não a consulta. Com o qwen isso ficou bem mais raro que era com o llama, mas ainda acontece.

E a terceira é que não tem GPU e o modelo é pequeno. Não tem ajuste de código que resolva isso, a velocidade é limitada pelo hardware e pelo tamanho do modelo. Um modelo maior melhora a fidelidade, mas a velocidade continua presa na CPU.

A decisão de escopo foi passar toda resposta pelo LLM, que é a arquitetura que o teste pede. Se um dia fosse pra encarar lista grande e exata, o caminho seria renderizar o resultado da tool direto na tela, sem o modelo ter que redigitar item por item.
