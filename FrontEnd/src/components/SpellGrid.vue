<!-- Spell List Component -->
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useSpellStore } from '@/stores/spellStore'
import SpellModal from '@/components/SpellModal.vue'

const store = useSpellStore()
const fetchSpells = () => store.fetchSpells(store.currentPage)
const applyFilters = () => {
  store.fetchSpells(1)
}

// Modal state
const isModalOpen = ref(false)
const selectedSpellId = ref('')

const openModal = (spellId: string) => {
  selectedSpellId.value = spellId
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
}

const sortBy = (column: string) => {
  if (store.sortColumn === column) {
    // Toggle sort direction if clicking the same column
    store.sortDirection = store.sortDirection === 'asc' ? 'desc' : 'asc'
  } else {
    // Default to ascending when selecting a new column
    store.sortColumn = column
    store.sortDirection = 'asc'
  }
  store.fetchSpells(1) // Reset to first page when sorting
}

// Get sort indicator arrow for column headers
const getSortIndicator = (column: string) => {
  if (store.sortColumn !== column) return ''
  return store.sortDirection === 'asc' ? ' ↑' : ' ↓'
}

onMounted(fetchSpells)
</script>
<template>
  <div class="min-h-screen bg-gray-50 p-4">
    <div class="max-w-6xl mx-auto">
      <!-- Header -->
      <h1 class="text-5xl font-extrabold text-center mb-6 tracking-tight text-sky-600">
        D&D 5e Spells
      </h1>

      <!-- Search Section -->
      <div class="bg-white rounded-lg shadow-md p-6 mb-6">
        <div class="flex flex-col gap-4 sm:flex-row">
          <input v-model="store.nameFilter" placeholder="Search by spell name..."
            class="flex-1 px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-sky-600 focus:border-transparent outline-none transition-all" />
          <button @click="applyFilters"
            class="px-6 py-2 bg-sky-600 text-white rounded-lg hover:opacity-90 transition-opacity font-medium">
            Search
          </button>
        </div>
      </div>

      <!-- Spells Table -->
      <div class="bg-white rounded-lg shadow-md overflow-hidden">
        <table class="w-full">
          <thead>
            <tr class="bg-sky-600 text-white">
              <th @click="sortBy('name')"
                class="px-6 py-4 text-left font-semibold w-1/3 cursor-pointer hover:bg-sky-700">
                Name{{ getSortIndicator('name') }}
              </th>
              <th @click="sortBy('level')"
                class="px-6 py-4 text-left font-semibold w-1/6 cursor-pointer hover:bg-sky-700">
                Level{{ getSortIndicator('level') }}
              </th>
              <th @click="sortBy('class')"
                class="px-6 py-4 text-left font-semibold w-1/3 cursor-pointer hover:bg-sky-700">
                Class{{ getSortIndicator('class') }}
              </th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200">
            <tr v-for="spell in store.spells" :key="spell.id" class="hover:bg-gray-50 transition-colors cursor-pointer"
              @click="openModal(spell.id)">
              <td class="px-6 py-4 w-1/3">
                <span class="text-sky-600 hover:text-sky-800 font-medium">
                  {{ spell.name }}
                </span>
              </td>
              <td class="px-6 py-4 w-1/6">
                <span class="px-3 py-1 bg-sky-100 text-sky-700 rounded-full text-sm font-semibold">
                  Level {{ spell.level }}
                </span>
              </td>
              <td class="px-6 py-4 text-gray-600 w-1/3">
                <span v-if="spell.classIds?.length">
                  {{ spell.classIds.join(', ') }}
                </span>
                <span v-else>-</span>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- No Results Message -->
        <div v-if="store.spells.length === 0" class="py-8 text-center text-gray-500">
          No spells found matching your criteria
        </div>

        <!-- Pagination -->
        <div class="bg-gray-50 px-6 py-4 border-t border-gray-200">
          <div class="flex items-center justify-between">
            <button @click="store.fetchSpells(store.currentPage - 1)" :disabled="store.currentPage === 1"
              class="px-4 py-2 rounded-md text-sm font-medium text-gray-700 border border-gray-300 bg-white hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed">
              Previous
            </button>
            <span class="text-sm text-gray-700">
              Page {{ store.currentPage }} of {{ store.totalPages }}
            </span>
            <button @click="store.fetchSpells(store.currentPage + 1)" :disabled="store.currentPage === store.totalPages"
              class="px-4 py-2 rounded-md text-sm font-medium text-gray-700 border border-gray-300 bg-white hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed">
              Next
            </button>
          </div>
        </div>
      </div>

      <!-- Spell Modal -->
      <SpellModal :is-open="isModalOpen" :spell-id="selectedSpellId" @close="closeModal" />

    </div>
  </div>
</template>