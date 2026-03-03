'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import toast from 'react-hot-toast';
import GodForm from '@/components/gods/GodForm';
import { God, Mythology, GodInput } from '@/lib/types';
import { updateGod } from '@/lib/api/gods';

interface EditGodClientProps {
  god: God;
  mythologies: Mythology[];
}

export default function EditGodClient({ god, mythologies }: EditGodClientProps) {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (data: GodInput) => {
    setIsSubmitting(true);
    try {
      await updateGod(god.id, data);
      toast.success('God updated successfully');
      router.push(`/gods/${god.id}`);
    } catch (error: any) {
      toast.error(error.message || 'Failed to update god');
      console.error('Update error:', error);
      throw error;
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <GodForm god={god} mythologies={mythologies} onSubmit={handleSubmit} isSubmitting={isSubmitting} />
  );
}
