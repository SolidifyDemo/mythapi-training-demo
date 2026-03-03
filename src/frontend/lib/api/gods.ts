import apiClient from './client';
import { God, GodInput } from '../types';

/**
 * Fetch all gods
 */
export async function getGods(): Promise<God[]> {
  const response = await apiClient.get<God[]>('/gods');
  return response.data;
}

/**
 * Fetch a single god by ID
 */
export async function getGodById(id: number): Promise<God> {
  const response = await apiClient.get<God>(`/gods/${id}`);
  return response.data;
}

/**
 * Search gods by name
 */
export async function searchGods(
  name: string,
  includeAliases: boolean = false
): Promise<God[]> {
  const response = await apiClient.get<God[]>(`/gods/search/${encodeURIComponent(name)}`, {
    params: { includeAliases },
  });
  return response.data;
}

/**
 * Create a new god
 */
export async function createGod(godData: Omit<GodInput, 'id'>): Promise<God[]> {
  const response = await apiClient.post<God[]>('/gods', [
    { ...godData, id: null },
  ]);
  return response.data;
}

/**
 * Update an existing god
 */
export async function updateGod(id: number, godData: Omit<GodInput, 'id'>): Promise<God[]> {
  const response = await apiClient.post<God[]>('/gods', [
    { ...godData, id },
  ]);
  return response.data;
}

/**
 * Delete a god by ID
 */
export async function deleteGod(id: number): Promise<void> {
  await apiClient.delete(`/gods/${id}`);
}

/**
 * Batch upsert gods (create or update multiple)
 */
export async function batchUpsertGods(gods: GodInput[]): Promise<God[]> {
  const response = await apiClient.post<God[]>('/gods', gods);
  return response.data;
}
