const CHAT_API_URL = process.env.NEXT_PUBLIC_CHAT_API_URL ?? 'http://localhost:5000'

export interface StreamHandlers {
  onToken: (delta: string) => void
  onError: (message: string) => void
  onDone: () => void
}

export const streamChat = async (
  sessionId: string,
  message: string,
  handlers: StreamHandlers,
): Promise<void> => {
  try {
    await openStream(sessionId, message, handlers)
  } catch {
    handlers.onError('Não foi possível conectar ao servidor. Verifique se o chat-api está no ar.')
  }
}

const openStream = async (sessionId: string, message: string, handlers: StreamHandlers) => {
  const response = await fetch(`${CHAT_API_URL}/chat`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ sessionId, message }),
  })

  if (!response.ok || !response.body) {
    handlers.onError(`Falha na conexão com o servidor (HTTP ${response.status}).`)
    return
  }

  await readStream(response.body, handlers)
}

const readStream = async (body: ReadableStream<Uint8Array>, handlers: StreamHandlers) => {
  const reader = body.getReader()
  const decoder = new TextDecoder()
  let buffer = ''

  while (true) {
    const { value, done } = await reader.read()
    if (done) break
    buffer += decoder.decode(value, { stream: true })
    const events = buffer.split('\n\n')
    buffer = events.pop() ?? ''
    if (drainEvents(events, handlers)) return
  }

  handlers.onDone()
}

const drainEvents = (events: string[], handlers: StreamHandlers): boolean => {
  for (const raw of events) {
    const payload = parseData(raw)
    if (payload === null) continue
    if (dispatch(payload, handlers)) return true
  }
  return false
}

const dispatch = (payload: string, handlers: StreamHandlers): boolean => {
  if (payload === '[DONE]') {
    handlers.onDone()
    return true
  }
  if (payload.startsWith('[ERROR]')) {
    handlers.onError(payload.slice('[ERROR]'.length).trim())
    return true
  }
  handlers.onToken(payload)
  return false
}

const parseData = (raw: string): string | null => {
  const lines = raw.split('\n').filter((line) => line.startsWith('data:'))
  if (lines.length === 0) return null
  return lines.map((line) => line.replace(/^data: ?/, '')).join('\n')
}
