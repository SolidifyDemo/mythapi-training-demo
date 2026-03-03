'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import toast from 'react-hot-toast';
import Button from '@/components/ui/Button';
import { deleteGod } from '@/lib/api/gods';

interface DeleteButtonProps {
  godId: number;
  godName: string;
}

export default function DeleteButton({ godId, godName }: DeleteButtonProps) {
  const router = useRouter();
  const [isDeleting, setIsDeleting] = useState(false);

  const handleDelete = async () => {
    if (!confirm(`Are you sure you want to delete ${godName}?`)) {
      return;
    }

    setIsDeleting(true);
    try {
      await deleteGod(godId);
      toast.success('God deleted successfully');
      router.push('/gods');
    } catch (error: any) {
      toast.error(error.message || 'Failed to delete god');
      console.error('Delete error:', error);
      setIsDeleting(false);
    }
  };

  return (
    <Button
      variant="danger"
      onClick={handleDelete}
      isLoading={isDeleting}
      disabled={isDeleting}
    >
      Delete God
    </Button>
  );
}
