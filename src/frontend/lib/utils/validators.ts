import { z } from 'zod';

/**
 * Validation schema for God form
 */
export const godSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100, 'Name must be less than 100 characters'),
  description: z
    .string()
    .min(1, 'Description is required')
    .max(500, 'Description must be less than 500 characters'),
  mythologyId: z.number().min(1, 'Please select a mythology'),
});

export type GodFormData = z.infer<typeof godSchema>;

/**
 * Validate god form data
 */
export function validateGodData(data: unknown): { success: boolean; data?: GodFormData; errors?: string[] } {
  try {
    const validated = godSchema.parse(data);
    return { success: true, data: validated };
  } catch (error) {
    if (error instanceof z.ZodError) {
      return {
        success: false,
        errors: error.errors.map((e) => e.message),
      };
    }
    return { success: false, errors: ['Validation failed'] };
  }
}
