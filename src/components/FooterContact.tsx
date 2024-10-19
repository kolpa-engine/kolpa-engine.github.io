import { Button } from "./ui/button";
import { Input } from "./ui/input";

export default function FooterContact() {
    return (
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
    )
}
