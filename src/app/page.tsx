"use client"

import { useState } from 'react'
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Code, Cpu, Gamepad2, Layers, Menu, Zap } from "lucide-react"

export default function LandingPage() {
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  return (
    <div className="min-h-screen bg-background text-foreground">
      <header className="container mx-auto px-4 py-6">
        <nav className="flex justify-between items-center">
          <div className="flex items-center space-x-4">
            <Gamepad2 className="h-8 w-8 text-primary" />
            <span className="text-2xl font-bold">Kolpa Engine</span>
          </div>
          <div className="hidden md:flex space-x-6">
            <a href="#features" className="hover:text-primary transition-colors">Features</a>
            <a href="#showcase" className="hover:text-primary transition-colors">Showcase</a>
            <a href="#pricing" className="hover:text-primary transition-colors">Pricing</a>
            <a href="#contact" className="hover:text-primary transition-colors">Contact</a>
          </div>
          <Button className="hidden md:inline-flex">Get Started</Button>
          <Button variant="ghost" className="md:hidden" onClick={() => setIsMenuOpen(!isMenuOpen)}>
            <Menu />
          </Button>
        </nav>
        {isMenuOpen && (
          <div className="mt-4 flex flex-col space-y-4 md:hidden">
            <a href="#features" className="hover:text-primary transition-colors">Features</a>
            <a href="#showcase" className="hover:text-primary transition-colors">Showcase</a>
            <a href="#pricing" className="hover:text-primary transition-colors">Pricing</a>
            <a href="#contact" className="hover:text-primary transition-colors">Contact</a>
            <Button>Get Started</Button>
          </div>
        )}
      </header>

      <main>
        <section className="container mx-auto px-4 py-20 text-center">
          <h1 className="text-5xl md:text-7xl font-bold mb-6">
            Unleash Your Game&apos;s Potential
          </h1>
          <p className="text-xl md:text-2xl mb-8 text-muted-foreground">
            Kolpa Engine: The next-gen game development platform for creators who dare to dream big.
          </p>
          <div className="flex justify-center space-x-4">
            <Button size="lg">Download Now</Button>
            <Button size="lg" variant="outline">View Documentation</Button>
          </div>
        </section>

        <section id="features" className="container mx-auto px-4 py-20">
          <h2 className="text-4xl font-bold mb-12 text-center">Powerful Features</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            <FeatureCard 
              icon={<Zap className="h-8 w-8 text-primary" />}
              title="Lightning Fast"
              description="Optimized performance for smooth gameplay and rapid development."
            />
            <FeatureCard 
              icon={<Layers className="h-8 w-8 text-primary" />}
              title="Flexible Architecture"
              description="Modular design allows for easy customization and expansion."
            />
            <FeatureCard 
              icon={<Code className="h-8 w-8 text-primary" />}
              title="Developer Friendly"
              description="Intuitive API and comprehensive documentation for all skill levels."
            />
            <FeatureCard 
              icon={<Cpu className="h-8 w-8 text-primary" />}
              title="Cross-Platform"
              description="Deploy your games on multiple platforms with ease."
            />
            <FeatureCard 
              icon={<Gamepad2 className="h-8 w-8 text-primary" />}
              title="Advanced Physics"
              description="Realistic physics simulation for immersive gameplay experiences."
            />
            <FeatureCard 
              icon={<Zap className="h-8 w-8 text-primary" />}
              title="Real-Time Editing"
              description="Make changes on the fly without interrupting your workflow."
            />
          </div>
        </section>

        <section id="showcase" className="bg-muted py-20">
          <div className="container mx-auto px-4">
            <h2 className="text-4xl font-bold mb-12 text-center">Games Powered by Kolpa</h2>
            <Tabs defaultValue="action" className="w-full">
              <TabsList className="grid w-full grid-cols-4 mb-8">
                <TabsTrigger value="action">Action</TabsTrigger>
                <TabsTrigger value="adventure">Adventure</TabsTrigger>
                <TabsTrigger value="strategy">Strategy</TabsTrigger>
                <TabsTrigger value="simulation">Simulation</TabsTrigger>
              </TabsList>
              <TabsContent value="action" className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <GameCard title="Neon Blitz" image="/placeholder.svg?height=200&width=300" />
                <GameCard title="Cyber Assault" image="/placeholder.svg?height=200&width=300" />
                <GameCard title="Quantum Break" image="/placeholder.svg?height=200&width=300" />
              </TabsContent>
              <TabsContent value="adventure" className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <GameCard title="Mystic Quest" image="/placeholder.svg?height=200&width=300" />
                <GameCard title="Ethereal Realms" image="/placeholder.svg?height=200&width=300" />
                <GameCard title="Chronos Shift" image="/placeholder.svg?height=200&width=300" />
              </TabsContent>
              <TabsContent value="strategy" className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <GameCard title="Galactic Empires" image="/placeholder.svg?height=200&width=300" />
                <GameCard title="Tactical Minds" image="/placeholder.svg?height=200&width=300" />
                <GameCard title="Civilization X" image="/placeholder.svg?height=200&width=300" />
              </TabsContent>
              <TabsContent value="simulation" className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <GameCard title="City Architect" image="/placeholder.svg?height=200&width=300" />
                <GameCard title="Flight Master" image="/placeholder.svg?height=200&width=300" />
                <GameCard title="Eco Tycoon" image="/placeholder.svg?height=200&width=300" />
              </TabsContent>
            </Tabs>
          </div>
        </section>

        <section id="pricing" className="container mx-auto px-4 py-20">
          <h2 className="text-4xl font-bold mb-12 text-center">Choose Your Plan</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <PricingCard 
              title="Indie"
              price="$19"
              features={[
                "Core engine features",
                "Community support",
                "Basic asset library",
                "1 project"
              ]}
            />
            <PricingCard 
              title="Pro"
              price="$49"
              features={[
                "All Indie features",
                "Advanced physics",
                "Extended asset library",
                "Priority support",
                "5 projects"
              ]}
              highlighted={true}
            />
            <PricingCard 
              title="Enterprise"
              price="Custom"
              features={[
                "All Pro features",
                "Custom integrations",
                "Dedicated support",
                "Unlimited projects",
                "On-site training"
              ]}
            />
          </div>
        </section>

        <section id="contact" className="bg-muted py-20">
          <div className="container mx-auto px-4">
            <h2 className="text-4xl font-bold mb-8 text-center">Get in Touch</h2>
            <p className="text-xl text-center mb-12 text-muted-foreground">Have questions? We&apos;re here to help you create amazing games.</p>
            <form className="max-w-lg mx-auto">
              <div className="grid grid-cols-1 gap-6">
                <Input type="text" placeholder="Your Name" />
                <Input type="email" placeholder="Your Email" />
                <textarea 
                  className="w-full px-3 py-2 text-foreground bg-background border rounded-lg focus:outline-none focus:ring-2 focus:ring-ring" 
                  rows={4} 
                  placeholder="Your Message"
                ></textarea>
                <Button type="submit" size="lg">Send Message</Button>
              </div>
            </form>
          </div>
        </section>
      </main>

      <footer className="bg-muted py-8">
        <div className="container mx-auto px-4">
          <div className="flex flex-col md:flex-row justify-between items-center">
            <div className="flex items-center space-x-4 mb-4 md:mb-0">
              <Gamepad2 className="h-8 w-8 text-primary" />
              <span className="text-2xl font-bold">Kolpa Engine</span>
            </div>
            <div className="flex space-x-6">
              <a href="#" className="hover:text-primary transition-colors">Privacy Policy</a>
              <a href="#" className="hover:text-primary transition-colors">Terms of Service</a>
              <a href="#" className="hover:text-primary transition-colors">Contact</a>
            </div>
          </div>
          <div className="mt-8 text-center text-muted-foreground">
            © {new Date().getFullYear()} Kolpa Engine. All rights reserved.
          </div>
        </div>
      </footer>
    </div>
  )
}

function FeatureCard({ icon, title, description }) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="mb-4">{icon}</div>
        <h3 className="text-xl font-semibold mb-2">{title}</h3>
        <p className="text-muted-foreground">{description}</p>
      </CardContent>
    </Card>
  )
}

function GameCard({ title, image }) {
  return (
    <Card className="overflow-hidden">
      <img src={image} alt={title} className="w-full h-48 object-cover" />
      <CardContent className="p-4">
        <h3 className="text-lg font-semibold">{title}</h3>
      </CardContent>
    </Card>
  )
}

function PricingCard({ title, price, features, highlighted = false }) {
  return (
    <Card className={highlighted ? 'border-primary' : ''}>
      <CardContent className="p-6">
        <h3 className="text-2xl font-bold mb-4">{title}</h3>
        <p className="text-4xl font-bold mb-6">{price}<span className="text-xl font-normal">/month</span></p>
        <ul className="space-y-2 mb-6">
          {features.map((feature, index) => (
            <li key={index} className="flex items-center">
              <Zap className="h-5 w-5 text-primary mr-2" />
              {feature}
            </li>
          ))}
        </ul>
        <Button className="w-full" variant={highlighted ? "default" : "outline"}>
          Choose Plan
        </Button>
      </CardContent>
    </Card>
  )
}