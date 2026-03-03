'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import GodCard from '@/components/gods/GodCard';
import SearchBar from '@/components/ui/SearchBar';
import LoadingSpinner from '@/components/ui/LoadingSpinner';
import { God } from '@/lib/types';
import { searchGods, deleteGod } from '@/lib/api/gods';
import { debounce } from '@/lib/utils/formatters';

interface GodsListClientProps {
  initialGods: God[];
}

export default function GodsListClient({ initialGods }: GodsListClientProps) {
  const router = useRouter();
  const [gods, setGods] = useState<God[]>(initialGods);
  const [isSearching, setIsSearching] = useState(false);
  const [currentQuery, setCurrentQuery] = useState('');

  const handleSearch = debounce(async (query: string) => {
    setCurrentQuery(query);
    
    if (!query.trim()) {
      // Restore full list
      setGods(initialGods);
      return;
    }

    setIsSearching(true);
    try {
      const results = await searchGods(query, true);
      setGods(results);
    } catch (error: any) {
      toast.error(error.message || 'Failed to search gods');
      console.error('Search error:', error);
    } finally {
      setIsSearching(false);
    }
  }, 300);

  const handleDelete = async (id: number) => {
    try {
      await deleteGod(id);
      toast.success('God deleted successfully');
      
      // If searching, refresh search results
      if (currentQuery) {
        const results = await searchGods(currentQuery, true);
        setGods(results);
      } else {
        // Otherwise refresh the page to get updated list
        router.refresh();
      }
    } catch (error: any) {
      toast.error(error.message || 'Failed to delete god');
      console.error('Delete error:', error);
    }
  };

  return (
    <div>
      <div className="mb-8">
        <SearchBar
          onSearch={handleSearch}
          placeholder="Search gods by name or alias..."
        />
      </div>

      {isSearching ? (
        <div className="py-12">
          <LoadingSpinner size="lg" />
        </div>
      ) : gods.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">
            {currentQuery ? 'No gods found matching your search.' : 'No gods available.'}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {gods.map((god) => (
            <GodCard key={god.id} god={god} onDelete={handleDelete} />
          ))}
        </div>
      )}
    </div>
  );
}
