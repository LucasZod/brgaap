export type Role = 'user' | 'assistant'

export interface Message {
  id: string
  role: Role
  content: string
  isComplete: boolean
  error?: string
}
