import Card from '@/components/ui/Card';
import { getMythologies } from '@/lib/api/mythologies';

export default async function MythologiesPage() {
  const mythologies = await getMythologies();

  return (
    <div>
      <h1 className="text-4xl font-bold text-gray-900 mb-8">Mythologies</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {mythologies.map((mythology) => (
          <Card key={mythology.id}>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">{mythology.name}</h2>
            <p className="text-sm text-gray-500 mb-3">{mythology.region}</p>
            <p className="text-gray-600">{mythology.description}</p>
          </Card>
        ))}
      </div>

      {mythologies.length === 0 && (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">No mythologies available.</p>
        </div>
      )}
    </div>
  );
}
