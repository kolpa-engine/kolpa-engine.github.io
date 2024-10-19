"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Gamepad2, Menu } from "lucide-react"
import Link from "next/link"

export default function Header() {
    const [isMenuOpen, setIsMenuOpen] = useState(false)

    return (
        <header className="container mx-auto px-4 py-6">
            <nav className="flex justify-between items-center">
                <div className="flex items-center space-x-4">
                    <Gamepad2 className="h-8 w-8 text-primary" />
                    <span className="text-2xl font-bold">Kolpa Engine</span>
                </div>
                <div className="hidden md:flex space-x-6">
                    <Link href="#features" className="text-muted-foreground hover:text-primary transition-colors">Features</Link>
                    <Link href="#showcase" className="text-muted-foreground hover:text-primary transition-colors">Showcase</Link>
                    <Link href="#pricing" className="text-muted-foreground hover:text-primary transition-colors">Pricing</Link>
                    <Link href="#contact" className="text-muted-foreground hover:text-primary transition-colors">Contact</Link>
                    <Link href="/about" className="text-muted-foreground hover:text-primary transition-colors">About</Link>
                </div>
                <Button className="hidden md:inline-flex">Get Started</Button>
                <Button variant="ghost" className="md:hidden" onClick={() => setIsMenuOpen(!isMenuOpen)}>
                    <Menu />
                </Button>
            </nav>
            {isMenuOpen && (
                <div className="mt-4 flex flex-col space-y-4 md:hidden">
                    <Link href="#features" className="text-muted-foreground hover:text-primary transition-colors">Features</Link>
                    <Link href="#showcase" className="text-muted-foreground hover:text-primary transition-colors">Showcase</Link>
                    <Link href="#pricing" className="text-muted-foreground hover:text-primary transition-colors">Pricing</Link>
                    <Link href="#contact" className="text-muted-foreground hover:text-primary transition-colors">Contact</Link>
                    <Link href="/about" className="text-muted-foreground hover:text-primary transition-colors">About</Link>
                    <Button>Get Started</Button>
                </div>
            )}
        </header>
    )
}