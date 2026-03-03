import Link from 'next/link';
import Button from '@/components/ui/Button';

export default function NotFound() {
  return (
    <div className="flex flex-col items-center justify-center py-12">
      <h1 className="text-6xl font-bold text-gray-900 mb-4">404</h1>
      <h2 className="text-2xl font-semibold text-gray-700 mb-6">God Not Found</h2>
      <p className="text-gray-600 mb-8">The god you're looking for doesn't exist or has been removed.</p>
      <Link href="/gods">
        <Button variant="primary">Back to Gods</Button>
      </Link>
    </div>
  );
}
