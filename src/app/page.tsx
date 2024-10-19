import { ReactNode } from "react"
import Image from "next/image"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Code, Cpu, Gamepad2, Layers, MemoryStick, Zap } from "lucide-react"
import Footer from "@/components/Footer"
import Header from "@/components/Header"
import FooterContact from "@/components/FooterContact"

interface FeatureCardProps {
  icon: ReactNode
  title: string
  description: string
}

interface GameCardProps {
  title: string
  image: string
}

interface PricingCardProps {
  title: string
  price: string
  features: string[]
  highlighted?: boolean
}

export default function Index() {
  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header />

      <main>
        <section className="container mx-auto px-4 py-20 text-center">
          <h1 className="text-5xl md:text-7xl font-bold mb-6">
            Free and open source game engine for everyone.
          </h1>
          <p className="text-xl md:text-2xl mb-8 text-muted-foreground">
            Kolpa Engine: Pioneering the Future of Game Development with Kotlin Multiplatform.
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
              icon={<Cpu className="h-8 w-8 text-primary" />}
              title="Kotlin Multiplatform"
              description="Seamless cross-platform game development using Kotlin."
            />
            <FeatureCard
              icon={<Layers className="h-8 w-8 text-primary" />}
              title="Modular Design"
              description="Add only the components your project needs."
            />
            <FeatureCard
              icon={<Zap className="h-8 w-8 text-primary" />}
              title="Optimized Rendering"
              description="Smooth performance across all platforms."
            />
            <FeatureCard
              icon={<Code className="h-8 w-8 text-primary" />}
              title="Open-Source"
              description="Full control with open-source flexibility."
            />
            <FeatureCard
              icon={<MemoryStick className="h-8 w-8 text-primary" />}
              title="Efficient Memory Usage"
              description="Optimized memory management for large assets."
            />
            <FeatureCard
              icon={<Gamepad2 className="h-8 w-8 text-primary" />}
              title="Built-in Physics"
              description="Precision physics for realistic movement."
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
                <GameCard title="Example" image="/favicon.ico" />
                <GameCard title="Example" image="/favicon.ico" />
                <GameCard title="Example" image="/favicon.ico" />
              </TabsContent>
              <TabsContent value="adventure" className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <GameCard title="Example" image="/favicon.ico" />
                <GameCard title="Example" image="/favicon.ico" />
                <GameCard title="Example" image="/favicon.ico" />
              </TabsContent>
              <TabsContent value="strategy" className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <GameCard title="Example" image="/favicon.ico" />
                <GameCard title="Example" image="/favicon.ico" />
                <GameCard title="Example" image="/favicon.ico" />
              </TabsContent>
              <TabsContent value="simulation" className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <GameCard title="Example" image="/favicon.ico" />
                <GameCard title="Example" image="/favicon.ico" />
                <GameCard title="Example" image="/favicon.ico" />
              </TabsContent>
            </Tabs>
          </div>
        </section>

        <section id="pricing" className="container mx-auto px-4 py-20">
          <h2 className="text-4xl font-bold mb-12 text-center">Choose Your Plan</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <PricingCard
              title="Indie"
              price="$0"
              features={[
                "Core engine features",
                "Community support",
                "Basic asset library",
                "25 projects"
              ]}
            />
            <PricingCard
              title="Pro"
              price="$18"
              features={[
                "All Indie features",
                "Advanced physics",
                "Extended asset library",
                "Priority support",
                "1.000 projects"
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

        <FooterContact />
      </main>

      <Footer />
    </div>
  )
}

function FeatureCard({ icon, title, description }: FeatureCardProps) {
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

function GameCard({ title, image }: GameCardProps) {
  return (
    <Card className="overflow-hidden">
      <Image src={image} alt={title} width={300} height={200} className="w-full h-48 object-cover" />
      <CardContent className="p-4">
        <h3 className="text-lg font-semibold">{title}</h3>
      </CardContent>
    </Card>
  )
}

function PricingCard({ title, price, features, highlighted = false }: PricingCardProps) {
  return (
    <Card className={highlighted ? "border-primary" : ""}>
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
