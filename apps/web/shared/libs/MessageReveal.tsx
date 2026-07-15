'use client'

import { motion, useReducedMotion } from 'motion/react'

export const MessageReveal = ({ children }: { children: React.ReactNode }) => {
  const reduce = useReducedMotion()
  return (
    <motion.div
      initial={reduce ? false : REVEAL_INITIAL}
      animate={REVEAL_ANIMATE}
      transition={REVEAL_TRANSITION}
    >
      {children}
    </motion.div>
  )
}

const REVEAL_INITIAL = { opacity: 0, y: 8 }
const REVEAL_ANIMATE = { opacity: 1, y: 0 }
const REVEAL_TRANSITION = { duration: 0.32, ease: [0.22, 1, 0.36, 1] as const }
