import Link from 'next/link';
import { PencilIcon, ArrowLeftIcon } from '@heroicons/react/24/outline';
import Card from '@/components/ui/Card';
import Button from '@/components/ui/Button';
import DeleteButton from './DeleteButton';
import { getGodById } from '@/lib/api/gods';
import { getMythologyById } from '@/lib/api/mythologies';
import { notFound } from 'next/navigation';

interface GodDetailPageProps {
  params: Promise<{ id: string }>;
}

export default async function GodDetailPage({ params }: GodDetailPageProps) {
  const { id } = await params;
  const godId = parseInt(id);

  if (isNaN(godId)) {
    notFound();
  }

  let god;
  let mythology;

  try {
    god = await getGodById(godId);
    mythology = await getMythologyById(god.mythologyId);
  } catch (error: any) {
    if (error.status === 404) {
      notFound();
    }
    throw error;
  }

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
        <div className="space-y-6">
          <div className="flex justify-between items-start">
            <h1 className="text-4xl font-bold text-gray-900">{god.name}</h1>
            <div className="flex gap-2">
              <Link href={`/gods/${god.id}/edit`}>
                <Button variant="secondary">
                  <PencilIcon className="h-5 w-5 mr-2" />
                  Edit
                </Button>
              </Link>
              <DeleteButton godId={god.id} godName={god.name} />
            </div>
          </div>

          <div>
            <h2 className="text-lg font-semibold text-gray-700 mb-2">Description</h2>
            <p className="text-gray-600 leading-relaxed">{god.description}</p>
          </div>

          <div>
            <h2 className="text-lg font-semibold text-gray-700 mb-2">Mythology</h2>
            <p className="text-gray-600">{mythology.name} ({mythology.region})</p>
            <p className="text-gray-500 text-sm mt-1">{mythology.description}</p>
          </div>

          {god.aliases && god.aliases.length > 0 && (
            <div>
              <h2 className="text-lg font-semibold text-gray-700 mb-2">Aliases</h2>
              <div className="flex flex-wrap gap-2">
                {god.aliases.map((alias) => (
                  <span
                    key={alias.id}
                    className="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-sm"
                  >
                    {alias.name}
                  </span>
                ))}
              </div>
            </div>
          )}

          <div className="pt-4 border-t border-gray-200">
            <p className="text-sm text-gray-500">God ID: {god.id}</p>
          </div>
        </div>
      </Card>
    </div>
  );
}
