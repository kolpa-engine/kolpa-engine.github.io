import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"
import { Github, Twitter, Linkedin, Code, Users, BookOpen, Zap } from "lucide-react"
import Link from "next/link"
import { ReactNode } from "react"
import Header from "@/components/Header"
import Footer from "@/components/Footer"

export default function About() {
  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header />

      <main className="container mx-auto px-4 py-12">
        <section id="about" className="mb-20">
          <h1 className="text-4xl font-bold mb-6">About Kolpa Engine</h1>
          <p className="text-xl mb-8 text-muted-foreground">
            Kolpa Engine is an open-source game development platform designed to empower creators and push the boundaries of interactive experiences.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <FeatureCard
              icon={<Code className="h-6 w-6" />}
              title="Open Source"
              description="Built by the community, for the community. Contribute and shape the future of game development."
            />
            <FeatureCard
              icon={<Zap className="h-6 w-6" />}
              title="High Performance"
              description="Optimized for speed and efficiency, allowing you to create stunning games without compromise."
            />
            <FeatureCard
              icon={<Users className="h-6 w-6" />}
              title="Community Driven"
              description="Join a vibrant community of developers, artists, and designers all working towards a common goal."
            />
            <FeatureCard
              icon={<BookOpen className="h-6 w-6" />}
              title="Extensive Documentation"
              description="Comprehensive guides and API references to help you get started and master Kolpa Engine."
            />
          </div>
        </section>

        <section id="team" className="mb-20">
          <h2 className="text-3xl font-bold mb-6">Meet the Team</h2>
          <p className="text-xl mb-8 text-muted-foreground">
            Kolpa Engine would not be possible without the help of its dedicated team of developers and contributors.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            <TeamMember
              name="Gabriel L. Bezerra"
              role="Lead Developer"
              avatar="https://github.com/gabriel-aplok.png"
              github="https://github.com/gabriel-aplok"
              twitter="https://x.com/GabrielAplok"
              linkedin="https://linkedin.com/in/gabrielaplok/"
            />
            <TeamMember
              name="Irineu A. Silva"
              role="Lead Developer @ kolpa-engine"
              avatar="https://github.com/Irineu333.png"
              github="https://github.com/Irineu333"
            />
          </div>
        </section>

        <section id="community" className="mb-20">
          <h2 className="text-3xl font-bold mb-6">Join Our Community</h2>
          <p className="text-xl mb-8 text-muted-foreground">
            Kolpa Engine thrives on community contributions. Here&apos;s how you can get involved:
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <Card>
              <CardContent className="p-6">
                <h3 className="text-2xl font-semibold mb-4">Contribute</h3>
                <p className="mb-4">Help us improve Kolpa Engine by contributing code, reporting bugs, or suggesting new features.</p>
                <Button asChild>
                  <Link href="https://github.com/kolpa-engine/kolpa/contribute">Start Contributing</Link>
                </Button>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-6">
                <h3 className="text-2xl font-semibold mb-4">Discuss</h3>
                <p className="mb-4">Join our forums to discuss game development, share your projects, and get help from the community.</p>
                <Button asChild>
                  <Link href="https://forum.kolpa-engine.org">Join the Discussion</Link>
                </Button>
              </CardContent>
            </Card>
          </div>
        </section>

        <section id="roadmap" className="mb-20">
          <h2 className="text-3xl font-bold mb-6">Roadmap</h2>
          <p className="text-xl mb-8 text-muted-foreground">
            We&apos;re constantly working to improve Kolpa Engine. Here&apos;s what we&apos;re planning for the future:
          </p>
          <div className="space-y-4">
            <RoadmapItem
              title="Kotlin Multiplatform Integration"
              description="Enhancing support for seamless cross-platform development using Kotlin Multiplatform to target multiple platforms efficiently."
              status="In Progress"
            />
            <RoadmapItem
              title="Performance Optimization Tools"
              description="Implementing advanced optimization techniques and profiling tools to ensure the best performance across all platforms."
              status="Planned"
            />
            <RoadmapItem
              title="Expanded Asset Library"
              description="Developing a comprehensive asset library with high-quality assets, including textures, models, and audio, for faster game development."
              status="Under Consideration"
            />

          </div>
        </section>
      </main>

      <Footer />
    </div>
  )
}

interface FeatureCardProps {
  icon: ReactNode;
  title: string;
  description: string;
}

function FeatureCard({ icon, title, description }: FeatureCardProps) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="mb-4 text-primary">{icon}</div>
        <h3 className="text-xl font-semibold mb-2">{title}</h3>
        <p className="text-muted-foreground">{description}</p>
      </CardContent>
    </Card>
  )
}

interface TeamMemberProps {
  name: string;
  role: string;
  avatar: string;
  github: string;
  twitter?: string;
  linkedin?: string;
}

function TeamMember({ name, role, avatar, github, twitter, linkedin }: TeamMemberProps) {
  return (
    <Card>
      <CardContent className="p-6 flex flex-col items-center text-center">
        <Avatar className="w-24 h-24 mb-4">
          <AvatarImage src={avatar} alt={name} />
          <AvatarFallback>{name.split(' ').map(n => n[0]).join('')}</AvatarFallback>
        </Avatar>
        <h3 className="text-xl font-semibold mb-1">{name}</h3>
        <p className="text-muted-foreground mb-4">{role}</p>
        <div className="flex space-x-4">
          <Link href={github || "#"} className="text-muted-foreground hover:text-primary">
            <Github className="h-5 w-5" />
          </Link>
          {twitter && (
            <Link href={twitter || "#"} className="text-muted-foreground hover:text-primary">
              <Twitter className="h-5 w-5" />
            </Link>
          )}
          {linkedin && (
            <Link href={linkedin || "#"} className="text-muted-foreground hover:text-primary">
              <Linkedin className="h-5 w-5" />
            </Link>
          )}
        </div>
      </CardContent>
    </Card>
  )
}

interface RoadmapItemProps {
  title: string;
  description: string;
  status: "Planned" | "Under Consideration" | "In Progress" | "Completed";
}

function RoadmapItem({ title, description, status }: RoadmapItemProps) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex justify-between items-start mb-2">
          <h3 className="text-xl font-semibold">{title}</h3>
          <Badge variant={status === "In Progress" ? "default" : status === "Planned" ? "secondary" : "outline"}>
            {status}
          </Badge>
        </div>
        <p className="text-muted-foreground">{description}</p>
      </CardContent>
    </Card>
  )
}