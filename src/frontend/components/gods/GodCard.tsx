'use client';

import Link from 'next/link';
import { TrashIcon, PencilIcon } from '@heroicons/react/24/outline';
import Card from '../ui/Card';
import Button from '../ui/Button';
import { GodCardProps } from '@/lib/types';
import { truncateText } from '@/lib/utils/formatters';

export default function GodCard({ god, onDelete }: GodCardProps) {
  const handleDelete = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (confirm(`Are you sure you want to delete ${god.name}?`)) {
      onDelete?.(god.id);
    }
  };

  return (
    <Card className="h-full flex flex-col">
      <div className="flex-1">
        <h3 className="text-xl font-bold text-gray-900 mb-2">{god.name}</h3>
        <p className="text-gray-600 mb-3">{truncateText(god.description, 150)}</p>
        {god.aliases && god.aliases.length > 0 && (
          <div className="mb-3">
            <span className="text-sm font-medium text-gray-700">Aliases: </span>
            <span className="text-sm text-gray-600">
              {god.aliases.map((a) => a.name).join(', ')}
            </span>
          </div>
        )}
      </div>
      <div className="flex gap-2 mt-4 pt-4 border-t border-gray-200">
        <Link href={`/gods/${god.id}`} className="flex-1">
          <Button variant="secondary" size="sm" className="w-full">
            View Details
          </Button>
        </Link>
        <Link href={`/gods/${god.id}/edit`}>
          <Button variant="ghost" size="sm">
            <PencilIcon className="h-4 w-4" />
          </Button>
        </Link>
        {onDelete && (
          <Button variant="danger" size="sm" onClick={handleDelete}>
            <TrashIcon className="h-4 w-4" />
          </Button>
        )}
      </div>
    </Card>
  );
}
