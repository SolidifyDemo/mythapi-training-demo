export interface Alias {
  id: number;
  godId: number;
  name: string;
}

export interface God {
  id: number;
  name: string;
  description: string;
  mythologyId: number;
  aliases: Alias[];
}

export interface GodInput {
  id?: number;
  name: string;
  description: string;
  mythologyId: number;
}
