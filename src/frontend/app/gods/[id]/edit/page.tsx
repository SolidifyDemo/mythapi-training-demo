import Link from 'next/link';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';
import EditGodClient from './EditGodClient';
import { getGodById } from '@/lib/api/gods';
import { getMythologies } from '@/lib/api/mythologies';
import { notFound } from 'next/navigation';

interface EditGodPageProps {
  params: Promise<{ id: string }>;
}

export default async function EditGodPage({ params }: EditGodPageProps) {
  const { id } = await params;
  const godId = parseInt(id);

  if (isNaN(godId)) {
    notFound();
  }

  let god;
  let mythologies;

  try {
    [god, mythologies] = await Promise.all([
      getGodById(godId),
      getMythologies(),
    ]);
  } catch (error: any) {
    if (error.status === 404) {
      notFound();
    }
    throw error;
  }

  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-6">
        <Link href={`/gods/${god.id}`}>
          <Button variant="ghost" size="sm">
            <ArrowLeftIcon className="h-4 w-4 mr-2" />
            Back to God Details
          </Button>
        </Link>
      </div>

      <Card>
        <h1 className="text-3xl font-bold text-gray-900 mb-6">Edit God: {god.name}</h1>
        <EditGodClient god={god} mythologies={mythologies} />
      </Card>
    </div>
  );
}
