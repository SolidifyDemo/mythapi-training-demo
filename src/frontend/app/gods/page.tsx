import Link from 'next/link';
import { PlusIcon } from '@heroicons/react/24/outline';
import Button from '@/components/ui/Button';
import GodsListClient from './GodsListClient';
import { getGods } from '@/lib/api/gods';

export default async function GodsPage() {
  const gods = await getGods();

  return (
    <div>
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-4xl font-bold text-gray-900">Gods</h1>
        <Link href="/gods/new">
          <Button variant="primary">
            <PlusIcon className="h-5 w-5 mr-2" />
            Create New God
          </Button>
        </Link>
      </div>

      <GodsListClient initialGods={gods} />
    </div>
  );
}
