import Link from 'next/link';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';
import CreateGodClient from './CreateGodClient';
import { getMythologies } from '@/lib/api/mythologies';

export default async function NewGodPage() {
  const mythologies = await getMythologies();

  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-6">
        <Link href="/gods">
          <Button variant="ghost" size="sm">
            <ArrowLeftIcon className="h-4 w-4 mr-2" />
            Back to Gods
          </Button>
        </Link>
      </div>

      <Card>
        <h1 className="text-3xl font-bold text-gray-900 mb-6">Create New God</h1>
        <CreateGodClient mythologies={mythologies} />
      </Card>
    </div>
  );
}
