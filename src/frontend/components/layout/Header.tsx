import Link from 'next/link';

export default function Header() {
  return (
    <header className="bg-white shadow-sm border-b border-gray-200">
      <div className="container mx-auto px-4 py-4">
        <div className="flex items-center justify-between">
          <Link href="/" className="flex items-center space-x-2">
            <h1 className="text-2xl font-bold text-blue-600">MythAPI</h1>
          </Link>
          <nav className="flex items-center space-x-6">
            <Link
              href="/gods"
              className="text-gray-700 hover:text-blue-600 font-medium transition-colors"
            >
              Gods
            </Link>
            <Link
              href="/mythologies"
              className="text-gray-700 hover:text-blue-600 font-medium transition-colors"
            >
              Mythologies
            </Link>
          </nav>
        </div>
      </div>
    </header>
  );
}
