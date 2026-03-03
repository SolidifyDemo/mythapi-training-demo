import apiClient from './client';
import { Mythology } from '../types';

/**
 * Fetch all mythologies
 */
export async function getMythologies(): Promise<Mythology[]> {
  const response = await apiClient.get<Mythology[]>('/mythologies');
  return response.data;
}

/**
 * Fetch a single mythology by ID
 */
export async function getMythologyById(id: number): Promise<Mythology> {
  const response = await apiClient.get<Mythology>(`/mythologies/${id}`);
  return response.data;
}

/**
 * Get mythology options for form dropdowns
 * Returns simplified data for select inputs
 */
export async function getMythologyOptions(): Promise<{ id: number; name: string }[]> {
  const mythologies = await getMythologies();
  return mythologies.map((m) => ({ id: m.id, name: m.name }));
}
