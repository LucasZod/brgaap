'use client'

import { Fragment, useEffect, useRef, type FormEvent, type KeyboardEvent } from 'react'
import Markdown from 'react-markdown'
import type { Components } from 'react-markdown'
import { useReducedMotion } from 'motion/react'
import { useChatStore, sendMessage } from '@/stores/chat.store'
import type { Message } from '@/types/chat'
import { IconButton } from '@/shared/ui/IconButton'
import { Textarea } from '@/shared/ui/Textarea'
import { MessageReveal } from '@/shared/libs/MessageReveal'
import { ThinkingDots } from '@/shared/libs/ThinkingDots'

export const Chat = () => (
  <ChatShell>
    <TopBar>
      <TopBarInner>
        <HeaderEyebrow />
        <HeaderTitle />
      </TopBarInner>
    </TopBar>
    <ConversationRegion>
      <ConversationColumn>
        <MessageThread />
      </ConversationColumn>
    </ConversationRegion>
    <ComposerBar>
      <ComposerColumn>
        <Composer />
      </ComposerColumn>
    </ComposerBar>
  </ChatShell>
)

const ChatShell = ({ children }: { children: React.ReactNode }) => (
  <div className='flex h-full flex-col'>{children}</div>
)

const TopBar = ({ children }: { children: React.ReactNode }) => (
  <header className='border-b border-hairline'>{children}</header>
)

const TopBarInner = ({ children }: { children: React.ReactNode }) => (
  <div className='mx-auto w-full max-w-[680px] px-6 py-5 text-center'>{children}</div>
)

const HeaderEyebrow = () => (
  <p className='font-sans text-[11px] font-medium uppercase tracking-[0.18em] text-ink-soft'>
    Assistente de consultas
  </p>
)

const HeaderTitle = () => (
  <h1 className='mt-1 font-serif text-2xl font-medium text-ink'>Gestão Pública</h1>
)

const ConversationRegion = ({ children }: { children: React.ReactNode }) => {
  const ref = useAutoScroll()
  return (
    <div ref={ref} aria-live='polite' aria-label='Conversa' className='flex-1 overflow-y-auto'>
      {children}
    </div>
  )
}

const ConversationColumn = ({ children }: { children: React.ReactNode }) => (
  <div className='mx-auto w-full max-w-[680px] px-6 py-10'>{children}</div>
)

const MessageThread = () => {
  const messages = useChatStore((state) => state.messages)
  return (
    <ThreadList>
      {messages.map((message) => (
        <MessageRow key={message.id} message={message} />
      ))}
    </ThreadList>
  )
}

const ThreadList = ({ children }: { children: React.ReactNode }) => (
  <div className='flex flex-col gap-8'>{children}</div>
)

const MessageRow = ({ message }: { message: Message }) => {
  if (message.role === 'user') return <UserRow message={message} />
  return <AssistantRow message={message} />
}

const UserRow = ({ message }: { message: Message }) => (
  <UserAlign>
    <MessageReveal>
      <UserBubble>{message.content}</UserBubble>
    </MessageReveal>
  </UserAlign>
)

const UserAlign = ({ children }: { children: React.ReactNode }) => (
  <div className='flex justify-end'>{children}</div>
)

const UserBubble = ({ children }: { children: React.ReactNode }) => (
  <div className='max-w-[85%] whitespace-pre-wrap rounded-2xl rounded-br-md bg-user-bubble px-4 py-2.5 font-sans text-[15px] leading-relaxed text-ink'>
    {children}
  </div>
)

const AssistantRow = ({ message }: { message: Message }) => (
  <MessageReveal>
    <AssistantBlock>
      <AssistantBody message={message} />
    </AssistantBlock>
  </MessageReveal>
)

const AssistantBlock = ({ children }: { children: React.ReactNode }) => (
  <div className='font-serif text-[17px] leading-[1.75] text-ink'>{children}</div>
)

const AssistantBody = ({ message }: { message: Message }) => {
  if (isPending(message)) return <ThinkingDots />
  return (
    <Fragment>
      <AssistantMarkdown content={message.content} />
      {!message.isComplete && <StreamingCaret />}
      {message.error && <ErrorNote text={message.error} />}
    </Fragment>
  )
}

const AssistantMarkdown = ({ content }: { content: string }) => (
  <Markdown components={MARKDOWN_COMPONENTS}>{content}</Markdown>
)

const StreamingCaret = () => <span className='streaming-caret' aria-hidden='true'>▍</span>

const ErrorNote = ({ text }: { text: string }) => (
  <p className='mt-3 font-sans text-[13px] italic text-ink-soft'>{text}</p>
)

const Composer = () => (
  <ComposerForm>
    <ComposerField />
    <SendControl />
  </ComposerForm>
)

const ComposerForm = ({ children }: { children: React.ReactNode }) => (
  <form
    onSubmit={handleSubmit}
    className='flex items-end gap-2 rounded-2xl border border-transparent bg-surface px-3 py-2 transition-colors duration-150 focus-within:border-focus'
  >
    {children}
  </form>
)

const ComposerField = () => {
  const input = useChatStore((state) => state.input)
  const setInput = useChatStore((state) => state.setInput)
  const isStreaming = useChatStore((state) => state.isStreaming)
  return (
    <Textarea
      value={input}
      onChange={setInput}
      onKeyDown={onEnterSubmit}
      placeholder='Digite sua mensagem ou comando...'
      disabled={isStreaming}
      autoFocus
      ariaLabel='Mensagem'
    />
  )
}

const SendControl = () => {
  const input = useChatStore((state) => state.input)
  const isStreaming = useChatStore((state) => state.isStreaming)
  const disabled = isStreaming || input.trim().length === 0
  return (
    <IconButton type='submit' label='Enviar mensagem' disabled={disabled} busy={isStreaming}>
      {isStreaming ? <StreamingGlyph /> : <SendArrow />}
    </IconButton>
  )
}

const SendArrow = () => (
  <svg width='16' height='16' viewBox='0 0 16 16' fill='none' aria-hidden='true'>
    <path
      d='M8 13V3M8 3L4 7M8 3l4 4'
      stroke='currentColor'
      strokeWidth='1.6'
      strokeLinecap='round'
      strokeLinejoin='round'
    />
  </svg>
)

const StreamingGlyph = () => <span className='h-2 w-2 rounded-[2px] bg-current' aria-hidden='true' />

const ComposerBar = ({ children }: { children: React.ReactNode }) => (
  <div className='border-t border-hairline bg-page'>{children}</div>
)

const ComposerColumn = ({ children }: { children: React.ReactNode }) => (
  <div className='mx-auto w-full max-w-[680px] px-6 py-4'>{children}</div>
)

const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
  event.preventDefault()
  void sendMessage()
}

const onEnterSubmit = (event: KeyboardEvent<HTMLTextAreaElement>) => {
  if (event.key !== 'Enter' || event.shiftKey) return
  event.preventDefault()
  void sendMessage()
}

const useAutoScroll = () => {
  const ref = useRef<HTMLDivElement>(null)
  const count = useChatStore((state) => state.messages.length)
  const tail = useChatStore((state) => lastContentLength(state.messages))
  const reduce = useReducedMotion()
  useEffect(() => {
    const element = ref.current
    if (!element) return
    element.scrollTo({ top: element.scrollHeight, behavior: reduce ? 'auto' : 'smooth' })
  }, [count, tail, reduce])
  return ref
}

const isPending = (message: Message): boolean => !message.isComplete && message.content.length === 0

const lastContentLength = (messages: Message[]): number =>
  messages.length === 0 ? 0 : messages[messages.length - 1].content.length

const MdHeading = ({ children }: { children: React.ReactNode }) => (
  <h2 className='mb-2 mt-6 font-serif text-xl font-semibold text-ink first:mt-0'>{children}</h2>
)

const MdSubheading = ({ children }: { children: React.ReactNode }) => (
  <h3 className='mb-1.5 mt-5 font-serif text-lg font-semibold text-ink first:mt-0'>{children}</h3>
)

const MdParagraph = ({ children }: { children: React.ReactNode }) => (
  <p className='mb-4 last:mb-0'>{children}</p>
)

const MdList = ({ children }: { children: React.ReactNode }) => (
  <ul className='mb-4 flex list-disc flex-col gap-1.5 pl-5 last:mb-0'>{children}</ul>
)

const MdOrderedList = ({ children }: { children: React.ReactNode }) => (
  <ol className='mb-4 flex list-decimal flex-col gap-1.5 pl-5 last:mb-0'>{children}</ol>
)

const MdListItem = ({ children }: { children: React.ReactNode }) => (
  <li className='pl-1 leading-relaxed'>{children}</li>
)

const MdStrong = ({ children }: { children: React.ReactNode }) => (
  <strong className='font-semibold text-ink'>{children}</strong>
)

const MdEmphasis = ({ children }: { children: React.ReactNode }) => (
  <em className='italic'>{children}</em>
)

const MdLink = ({ children, href }: { children: React.ReactNode; href?: string }) => (
  <a href={href} className='underline decoration-focus underline-offset-2 hover:text-ink-soft'>
    {children}
  </a>
)

const MdCode = ({ children }: { children: React.ReactNode }) => (
  <code className='rounded bg-surface px-1.5 py-0.5 font-mono text-[0.85em] text-ink'>{children}</code>
)

const MARKDOWN_COMPONENTS: Components = {
  h1: ({ children }) => <MdHeading>{children}</MdHeading>,
  h2: ({ children }) => <MdHeading>{children}</MdHeading>,
  h3: ({ children }) => <MdSubheading>{children}</MdSubheading>,
  p: ({ children }) => <MdParagraph>{children}</MdParagraph>,
  ul: ({ children }) => <MdList>{children}</MdList>,
  ol: ({ children }) => <MdOrderedList>{children}</MdOrderedList>,
  li: ({ children }) => <MdListItem>{children}</MdListItem>,
  strong: ({ children }) => <MdStrong>{children}</MdStrong>,
  em: ({ children }) => <MdEmphasis>{children}</MdEmphasis>,
  a: ({ children, href }) => <MdLink href={href}>{children}</MdLink>,
  code: ({ children }) => <MdCode>{children}</MdCode>,
}
