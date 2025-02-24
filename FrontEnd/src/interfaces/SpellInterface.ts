// Basic interfaces for the spell list view
export interface Spell {
  id: string;
  name: string;
  level: number;
  desc: string[];
  classIds?: string[]; 
  schoolId?: string;
}

// Base API response
export interface ApiResponse {
  spells: {
    $values: ApiSpell[];
  };
  totalItems: number;
  currentPage: number;
  totalPages: number;
}

// Simplified spell from API for list view
export interface ApiSpell {
  id: string;
  name: string;
  level: number;
  desc: {
    $values: string[];
  };
  school: {
    id: string;
    name: string;
  };
  classes: {
    $values: Array<{
      id: string;
      name: string;
    }>;
  };
}

// Detailed spell interface for modal view
export interface SpellDetail {
  id: string;
  name: string;
  desc: {
    $values: string[];
  };
  higherLevel?: {
    $values: string[];
  };
  range: string;
  components: {
    $values: string[];
  };
  material?: string;
  ritual: boolean;
  duration: string;
  concentration: boolean;
  castingTime: string;
  level: number;
  attackType?: string;
  damage?: {
    damageType: string | null;
    damageAtSlotLevel: {
      _0: string;
      _1: string;
      _2: string;
      _3: string;
      _4: string;
      _5: string;
      _6: string;
      _7: string;
      _8: string;
      _9: string;
    };
  };
  school: {
    id: string;
    name: string;
    url: string;
  };
  classes: {
    $values: Array<{
      id: string;
      name: string;
      url: string;
    }>;
  };
  subclasses: {
    $values: any[];
  };
  url: string;
  updatedAt: string;
}