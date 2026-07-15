'use client'

import { useEffect, useRef, type KeyboardEvent } from 'react'

interface TextareaProps {
  value: string
  onChange: (value: string) => void
  onKeyDown?: (event: KeyboardEvent<HTMLTextAreaElement>) => void
  placeholder?: string
  disabled?: boolean
  autoFocus?: boolean
  ariaLabel: string
}

export const Textarea = ({
  value,
  onChange,
  onKeyDown,
  placeholder,
  disabled,
  autoFocus,
  ariaLabel,
}: TextareaProps) => {
  const ref = useRef<HTMLTextAreaElement>(null)
  useAutoGrow(ref, value)

  return (
    <textarea
      ref={ref}
      rows={1}
      value={value}
      onChange={(event) => onChange(event.target.value)}
      onKeyDown={onKeyDown}
      placeholder={placeholder}
      disabled={disabled}
      autoFocus={autoFocus}
      aria-label={ariaLabel}
      className='max-h-40 w-full resize-none bg-transparent py-1.5 font-sans text-[15px] leading-relaxed text-ink placeholder:text-ink-soft focus:outline-none disabled:opacity-60'
    />
  )
}

const useAutoGrow = (ref: React.RefObject<HTMLTextAreaElement | null>, value: string) => {
  useEffect(() => {
    const element = ref.current
    if (!element) return
    element.style.height = 'auto'
    element.style.height = `${element.scrollHeight}px`
  }, [ref, value])
}
