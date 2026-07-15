interface IconButtonProps {
  label: string
  children: React.ReactNode
  onClick?: () => void
  type?: 'button' | 'submit'
  disabled?: boolean
  busy?: boolean
}

export const IconButton = ({
  label,
  children,
  onClick,
  type = 'button',
  disabled,
  busy,
}: IconButtonProps) => (
  <button
    type={type}
    onClick={onClick}
    disabled={disabled}
    aria-label={label}
    aria-busy={busy}
    className='inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-ink text-page transition-all duration-150 hover:-translate-y-px hover:bg-ink/90 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-focus active:translate-y-0 disabled:cursor-not-allowed disabled:bg-hairline disabled:text-ink-soft disabled:hover:translate-y-0'
  >
    {children}
  </button>
)
