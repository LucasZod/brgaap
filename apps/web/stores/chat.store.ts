import { create } from 'zustand'
import type { Message } from '@/types/chat'
import { streamChat } from '@/lib/chatClient'

const GREETING: Message = {
  id: 'greeting',
  role: 'assistant',
  content: 'Olá. Como posso ajudar com a gestão pública hoje?',
  isComplete: true,
}

export interface ChatStoreProps {
  messages: Message[]
  input: string
  isStreaming: boolean
  sessionId: string
}

export interface ChatStore extends ChatStoreProps {
  setInput: (value: string) => void
}

const initialState: ChatStoreProps = {
  messages: [GREETING],
  input: '',
  isStreaming: false,
  sessionId: '',
}

export const useChatStore = create<ChatStore>((set) => ({
  ...initialState,
  setInput: (value) => set({ input: value }),
}))

export const sendMessage = async () => {
  const { input, isStreaming } = useChatStore.getState()
  const text = input.trim()
  if (!text || isStreaming) return

  startTurn(text)
  const sessionId = ensureSessionId()
  await streamChat(sessionId, text, {
    onToken: appendDelta,
    onError: failAssistant,
    onDone: completeAssistant,
  })
}

const startTurn = (text: string) => {
  useChatStore.setState((state) => ({
    input: '',
    isStreaming: true,
    messages: [...state.messages, userMessage(text), assistantPlaceholder()],
  }))
}

const appendDelta = (delta: string) => {
  useChatStore.setState((state) => ({
    messages: patchLast(state.messages, (message) => ({
      ...message,
      content: message.content + delta,
    })),
  }))
}

const completeAssistant = () => {
  useChatStore.setState((state) => ({
    isStreaming: false,
    messages: patchLast(state.messages, (message) => ({ ...message, isComplete: true })),
  }))
}

const failAssistant = (reason: string) => {
  useChatStore.setState((state) => ({
    isStreaming: false,
    messages: patchLast(state.messages, (message) => ({
      ...message,
      isComplete: true,
      error: reason || 'Não foi possível concluir a resposta.',
    })),
  }))
}

const ensureSessionId = (): string => {
  const current = useChatStore.getState().sessionId
  if (current) return current
  const id = crypto.randomUUID()
  useChatStore.setState({ sessionId: id })
  return id
}

const userMessage = (text: string): Message => ({
  id: crypto.randomUUID(),
  role: 'user',
  content: text,
  isComplete: true,
})

const assistantPlaceholder = (): Message => ({
  id: crypto.randomUUID(),
  role: 'assistant',
  content: '',
  isComplete: false,
})

const patchLast = (messages: Message[], patch: (message: Message) => Message): Message[] =>
  messages.map((message, index) => (index === messages.length - 1 ? patch(message) : message))
