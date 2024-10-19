import type { Metadata } from "next"
import localFont from "next/font/local"
import "./globals.css"

const geistSans = localFont({
  src: "/fonts/GeistVF.woff",
  variable: "--font-geist-sans",
  weight: "100 900",
  display: "swap"
})

const geistMono = localFont({
  src: "/fonts/GeistMonoVF.woff",
  variable: "--font-geist-mono",
  weight: "100 900",
  display: "swap"
})

export const metadata: Metadata = {
  title: "Kolpa Engine - Multiplatform Game Engine Powered by Kotlin",
  description: "Redefining game development with Kotlin Multiplatform.",
  keywords: ["Kolpa Engine", "Kotlin", "Multiplatform", "Game Engine"],
  openGraph: {
    title: "Kolpa Engine - Multiplatform Game Engine",
    description: "Redefining game development with Kotlin Multiplatform.",
    url: "https://kolpaengine.com",
    siteName: "Kolpa Engine",
    images: [
      {
        url: "/favicon.ico",
        width: 128,
        height: 128,
        alt: "Kolpa Engine Logo"
      }
    ],
    locale: "en_US",
    type: "website"
  },
  twitter: {
    card: "summary_large_image",
    site: "@kolpaengine",
    title: "Kolpa Engine",
    description: "Redefining game development with Kotlin Multiplatform.",
    image: "/favicon.ico"
  },
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html lang="en">
      <body
        className={`${geistSans.variable} ${geistMono.variable} antialiased`}
      >
        {children}
      </body>
    </html>
  )
}