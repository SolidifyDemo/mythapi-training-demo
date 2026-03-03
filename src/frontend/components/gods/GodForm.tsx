'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Button from '../ui/Button';
import { GodFormProps } from '@/lib/types';
import { godSchema } from '@/lib/utils/validators';
import { formatErrorMessage } from '@/lib/utils/formatters';

export default function GodForm({ god, mythologies, onSubmit, isSubmitting = false }: GodFormProps) {
  const router = useRouter();
  const [formData, setFormData] = useState({
    name: god?.name || '',
    description: god?.description || '',
    mythologyId: god?.mythologyId || 0,
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string>('');

  useEffect(() => {
    if (god) {
      setFormData({
        name: god.name,
        description: god.description,
        mythologyId: god.mythologyId,
      });
    }
  }, [god]);

  const validateForm = (): boolean => {
    const result = godSchema.safeParse(formData);
    if (!result.success) {
      const newErrors: Record<string, string> = {};
      result.error.errors.forEach((err) => {
        if (err.path[0]) {
          newErrors[err.path[0].toString()] = err.message;
        }
      });
      setErrors(newErrors);
      return false;
    }
    setErrors({});
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError('');

    if (!validateForm()) {
      return;
    }

    try {
      await onSubmit(formData);
    } catch (error: any) {
      setSubmitError(formatErrorMessage(error));
    }
  };

  const handleCancel = () => {
    router.back();
  };

  return (
    <form onSubmit={handleSubmit} className="max-w-2xl mx-auto space-y-6">
      {submitError && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {submitError}
        </div>
      )}

      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-2">
          Name *
        </label>
        <input
          id="name"
          type="text"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.name ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="Enter god name"
        />
        {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name}</p>}
      </div>

      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
          Description *
        </label>
        <textarea
          id="description"
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          rows={4}
          className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.description ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="Enter god description"
        />
        {errors.description && <p className="mt-1 text-sm text-red-600">{errors.description}</p>}
      </div>

      <div>
        <label htmlFor="mythologyId" className="block text-sm font-medium text-gray-700 mb-2">
          Mythology *
        </label>
        <select
          id="mythologyId"
          value={formData.mythologyId}
          onChange={(e) => setFormData({ ...formData, mythologyId: parseInt(e.target.value) })}
          className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.mythologyId ? 'border-red-500' : 'border-gray-300'
          }`}
        >
          <option value={0}>Select a mythology</option>
          {mythologies.map((mythology) => (
            <option key={mythology.id} value={mythology.id}>
              {mythology.name}
            </option>
          ))}
        </select>
        {errors.mythologyId && <p className="mt-1 text-sm text-red-600">{errors.mythologyId}</p>}
      </div>

      <div className="flex gap-4 pt-4">
        <Button type="submit" variant="primary" isLoading={isSubmitting} className="flex-1">
          {god ? 'Update God' : 'Create God'}
        </Button>
        <Button type="button" variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
