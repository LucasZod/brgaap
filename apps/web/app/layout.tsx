import type { Metadata } from 'next'
import { Newsreader, Instrument_Sans } from 'next/font/google'
import './globals.css'

const newsreader = Newsreader({
  variable: '--font-newsreader',
  subsets: ['latin'],
  display: 'swap',
})

const instrument = Instrument_Sans({
  variable: '--font-instrument',
  subsets: ['latin'],
  display: 'swap',
})

export const metadata: Metadata = {
  title: 'Gestão Pública — Assistente',
  description:
    'Assistente de gestão pública brasileira. Consulte CNPJ de empresas, estados e municípios do IBGE em uma conversa.',
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang='pt-BR' className={`${newsreader.variable} ${instrument.variable} h-full`}>
      <body className='h-full'>{children}</body>
    </html>
  )
}
