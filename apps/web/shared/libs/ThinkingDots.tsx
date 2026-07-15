'use client'

import { motion, useReducedMotion } from 'motion/react'

export const ThinkingDots = () => {
  const reduce = useReducedMotion()
  return (
    <DotsRow>
      <Dot delay={0} still={reduce ?? false} />
      <Dot delay={0.18} still={reduce ?? false} />
      <Dot delay={0.36} still={reduce ?? false} />
    </DotsRow>
  )
}

const DotsRow = ({ children }: { children: React.ReactNode }) => (
  <span className='inline-flex items-center gap-1.5 py-2' role='status' aria-label='Consultando'>
    {children}
  </span>
)

const Dot = ({ delay, still }: { delay: number; still: boolean }) => (
  <motion.span
    className='h-1.5 w-1.5 rounded-full bg-ink-soft'
    animate={still ? undefined : DOT_ANIMATE}
    transition={{ ...DOT_TRANSITION, delay }}
  />
)

const DOT_ANIMATE = { opacity: [0.25, 1, 0.25] }
const DOT_TRANSITION = { duration: 1.1, repeat: Infinity, ease: 'easeInOut' as const }
