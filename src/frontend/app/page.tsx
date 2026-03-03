import Link from "next/link";
import Button from "@/components/ui/Button";

export default function Home() {
  return (
    <div className="max-w-4xl mx-auto text-center">
      <h1 className="text-5xl font-bold text-gray-900 mb-6">
        Welcome to MythAPI
      </h1>
      <p className="text-xl text-gray-600 mb-8">
        Explore gods and mythologies from around the world
      </p>
      <div className="flex gap-4 justify-center">
        <Link href="/gods">
          <Button variant="primary" size="lg">
            Browse Gods
          </Button>
        </Link>
        <Link href="/mythologies">
          <Button variant="secondary" size="lg">
            View Mythologies
          </Button>
        </Link>
      </div>
      
      <div className="mt-16 grid md:grid-cols-2 gap-8">
        <div className="bg-white p-6 rounded-lg shadow-md">
          <h2 className="text-2xl font-semibold text-gray-900 mb-3">Discover Gods</h2>
          <p className="text-gray-600">
            Browse and search through a comprehensive collection of deities from various mythologies.
          </p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow-md">
          <h2 className="text-2xl font-semibold text-gray-900 mb-3">Manage Collection</h2>
          <p className="text-gray-600">
            Create, update, and manage god entries with detailed descriptions and mythology associations.
          </p>
        </div>
      </div>
    </div>
  );
}
