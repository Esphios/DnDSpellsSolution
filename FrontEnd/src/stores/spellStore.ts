import { defineStore } from "pinia";
import { ref } from "vue";
import type { Spell, ApiResponse } from "@/interfaces/SpellInterface";
import spellService from "@/services/SpellService";

export const useSpellStore = defineStore("spellStore", {
  state: () => ({
    spells: [] as Spell[],
    totalItems: 0,
    totalPages: 1,
    currentPage: 1,
    nameFilter: ref(""),
    sortColumn: "name", // Default sort column
    sortDirection: "asc" as "asc" | "desc", // Default sort direction
  }),

  actions: {
    async fetchSpells(page = 1) {
      try {
        const response = await spellService.getSpells(
          page,
          this.nameFilter,
          this.sortColumn,
          this.sortDirection
        );
        const apiResponse = response.data as ApiResponse;

        // If the API still returns the extra "$values" wrapper for spells, extract it:
        const spellsArray = apiResponse.spells.$values;

        // Map the API spells to our Spell interface
        this.spells = spellsArray.map((spell) => ({
          id: spell.id,
          name: spell.name,
          level: spell.level,
          desc: spell.desc.$values, // assuming desc is still wrapped
          classIds: spell.classes.$values.map((c) => c.id),
          schoolId: spell.school.id,
        }));

        // Update pagination info from the API response
        this.totalItems = apiResponse.totalItems;
        this.currentPage = apiResponse.currentPage;
        this.totalPages = apiResponse.totalPages;
      } catch (error) {
        console.error("Failed to fetch spells:", error);
        this.spells = [];
      }
    },
  },
});
