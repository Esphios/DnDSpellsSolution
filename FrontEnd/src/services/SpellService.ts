import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL;

if (isNullOrEmpty(API_URL)) {
  throw new Error('VITE_API_URL must be configured before loading the frontend.');
}

const apiClient = axios.create({
  baseURL: API_URL,
  timeout: 5000,
  headers: {
    Accept: 'application/json',
  },
});

function isNullOrEmpty(value: string | null | undefined): boolean {
  return !value || value.trim().length === 0;
}

export default {
  async getSpells(page = 1, name = '', sortBy = 'name', sortDirection = 'asc') {
    let query = `?page=${page}&pageSize=10`; 
    
    if (!isNullOrEmpty(name)) {
      query += `&filter=${encodeURIComponent(name)}`;
    }
    
    // Add sorting parameters
    if (!isNullOrEmpty(sortBy)) {
      query += `&sortBy=${encodeURIComponent(sortBy)}`;
    }
    
    if (!isNullOrEmpty(sortDirection)) {
      query += `&sortDirection=${encodeURIComponent(sortDirection)}`;
    }
    
    return apiClient.get(query);
  },

  getSpell(id: string) {
    return apiClient.get(`/${id}`);
  },
};
