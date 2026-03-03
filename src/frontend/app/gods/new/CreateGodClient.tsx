'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import toast from 'react-hot-toast';
import GodForm from '@/components/gods/GodForm';
import { Mythology, GodInput } from '@/lib/types';
import { createGod } from '@/lib/api/gods';

interface CreateGodClientProps {
  mythologies: Mythology[];
}

export default function CreateGodClient({ mythologies }: CreateGodClientProps) {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (data: GodInput) => {
    setIsSubmitting(true);
    try {
      await createGod(data);
      toast.success('God created successfully');
      router.push('/gods');
    } catch (error: any) {
      toast.error(error.message || 'Failed to create god');
      console.error('Create error:', error);
      throw error;
    } finally {
      setIsSubmitting(false);
    }
  };

  return <GodForm mythologies={mythologies} onSubmit={handleSubmit} isSubmitting={isSubmitting} />;
}
