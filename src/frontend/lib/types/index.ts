// API Response Types
export interface God {
  id: number;
  name: string;
  description: string;
  mythologyId: number;
  aliases: Alias[];
}

export interface Alias {
  id: number;
  name: string;
  godId: number;
}

export interface Mythology {
  id: number;
  name: string;
  description: string;
  region: string;
}

// API Request Types
export interface GodInput {
  id?: number | null;
  name: string;
  description: string;
  mythologyId: number;
}

export interface CreateGodRequest {
  name: string;
  description: string;
  mythologyId: number;
}

export interface UpdateGodRequest {
  id: number;
  name: string;
  description: string;
  mythologyId: number;
}

// Component Props Types
export interface GodCardProps {
  god: God;
  onDelete?: (id: number) => void;
}

export interface GodFormProps {
  god?: God;
  mythologies: Mythology[];
  onSubmit: (data: GodInput) => Promise<void>;
  isSubmitting?: boolean;
}

export interface SearchBarProps {
  onSearch: (query: string) => void;
  placeholder?: string;
  defaultValue?: string;
}

// API Error Response
export interface ApiError {
  message: string;
  status: number;
  errors?: Record<string, string[]>;
}
