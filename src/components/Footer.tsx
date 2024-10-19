import { Gamepad2 } from "lucide-react"
import Link from "next/link"

export default function Footer() {
    return (
        <footer className="bg-muted py-8">
            <div className="container mx-auto px-4">
                <div className="flex flex-col md:flex-row justify-between items-center">
                    <div className="flex items-center space-x-4 mb-4 md:mb-0">
                        <Gamepad2 className="h-8 w-8 text-primary" />
                        <span className="text-2xl font-bold">Kolpa Engine</span>
                    </div>
                    <div className="flex space-x-6">
                        <Link href="/privacy" className="hover:text-primary transition-colors">Privacy Policy</Link>
                        <Link href="/terms" className="hover:text-primary transition-colors">Terms of Service</Link>
                        <Link href="/#contact" className="hover:text-primary transition-colors">Contact</Link>
                    </div>
                </div>
                <div className="mt-8 text-center text-muted-foreground">
                Copyright © {new Date().getFullYear()} Kolpa Engine & Contributors. All rights reserved.
                </div>
            </div>
        </footer>
    )
}
