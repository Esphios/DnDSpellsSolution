<!-- Spell Detail Modal -->
<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import spellService from '@/services/SpellService';
import type { SpellDetail } from '@/interfaces/SpellInterface';

const props = defineProps<{
  isOpen: boolean;
  spellId: string;
}>();

const emit = defineEmits(['close']);

const spell = ref<SpellDetail | null>(null);
const loading = ref(false);
const error = ref('');

// Format classes list
const classNames = computed(() => {
  if (!spell.value || !spell.value.classes || !spell.value.classes.$values) return '';
  return spell.value.classes.$values.map((c) => c.name).join(', ');
});


const isTableRow = (text: string) => {
  return text.trim().startsWith('|') && text.trim().endsWith('|');
};

const processContent = (paragraphs: string[]) => {
  const processed = [];
  let currentTable = [];
  let isCollectingTable = false;

  for (let i = 0; i < paragraphs.length; i++) {
    const paragraph = paragraphs[i].trim();

    if (isTableRow(paragraph)) {
      if (!isCollectingTable) {
        isCollectingTable = true;
      }
      currentTable.push(paragraph);
    } else {
      if (isCollectingTable) {
        // We've finished collecting a table
        processed.push({
          type: 'table',
          content: currentTable.join('\n')
        });
        currentTable = [];
        isCollectingTable = false;
      }
      processed.push({
        type: 'text',
        content: paragraph
      });
    }
  }

  // Handle any remaining table
  if (currentTable.length > 0) {
    processed.push({
      type: 'table',
      content: currentTable.join('\n')
    });
  }

  return processed;
};

const parseTable = (tableContent: string) => {
  const lines = tableContent.split('\n').filter(line => line.trim());

  // Parse headers
  const headers = lines[0]
    .split('|')
    .filter(cell => cell.trim())
    .map(cell => cell.trim());

  // Skip separator line and parse rows
  const rows = lines.slice(2)
    .map(line =>
      line
        .split('|')
        .filter(cell => cell.trim())
        .map(cell => cell.trim())
    );

  return {
    headers,
    rows
  };
};

// Close the modal
const closeModal = () => {
  emit('close');
};

// Fetch spell details
const fetchSpellDetails = async () => {
  if (!props.spellId) return;

  loading.value = true;
  error.value = '';

  try {
    const response = await spellService.getSpell(props.spellId);

    // Process the response
    const spellData = response.data;

    // Pre-process description for better formatting
    if (spellData.desc && spellData.desc.$values) {
      // Look for markdown-like tables in the description
      spellData.desc.$values = spellData.desc.$values.map((paragraph: string) => {
        // If paragraph has multiple lines and appears to contain a table
        if (paragraph.includes('\n') && paragraph.includes('|')) {
          return paragraph.replace(/\n/g, '\n\n'); // Add extra linebreaks for markdown parsing
        }
        return paragraph;
      });
    }

    spell.value = spellData;
  } catch (err) {
    console.error('Failed to fetch spell details:', err);
    error.value = 'Failed to load spell details. Please try again.';
  } finally {
    loading.value = false;
  }
};

// Watch for changes in the isOpen property
watch(() => props.isOpen, (newValue) => {
  if (newValue && props.spellId) {
    fetchSpellDetails();
  }
}, { immediate: true });
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 overflow-y-auto" aria-labelledby="modal-title" role="dialog"
    aria-modal="true">
    <!-- Background overlay (transparent) -->
    <div class="fixed inset-0 bg-black opacity-75" @click="closeModal"></div>

    <!-- Modal panel -->
    <div class="flex min-h-screen items-center justify-center p-4 text-center">
      <div
        class="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-4xl">
        <!-- Loading state -->
        <div v-if="loading" class="px-4 py-6 sm:p-6">
          <div class="flex items-center justify-center py-8">
            <div class="h-8 w-8 animate-spin rounded-full border-4 border-sky-600 border-t-transparent">
            </div>
          </div>
        </div>

        <!-- Error state -->
        <div v-else-if="error" class="px-4 py-6 sm:p-6">
          <div class="rounded-md bg-red-50 p-4">
            <div class="flex">
              <div class="text-red-600">{{ error }}</div>
            </div>
          </div>
          <div class="mt-6 flex justify-end">
            <button type="button" @click="closeModal"
              class="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-700">
              Close
            </button>
          </div>
        </div>

        <!-- Spell details -->
        <div v-else-if="spell" class="max-h-[80vh] overflow-y-auto">
          <!-- Header -->
          <div class="sticky top-0 bg-white sm:p-6 border-b border-gray-200 pb-4">
            <div class="flex items-center justify-between">
              <h3 class="text-2xl font-bold text-sky-600" id="modal-title">{{ spell.name }}</h3>
              <button @click="closeModal" class="rounded-md bg-white text-gray-400 hover:text-gray-500">
                <span class="sr-only">Close</span>
                <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            <div class="mt-1 flex flex-wrap gap-2">
              <span class="inline-flex rounded-full bg-sky-100 px-2.5 py-1 text-xs font-semibold text-sky-700">
                Level {{ spell.level }}
              </span>
              <span class="inline-flex rounded-full bg-sky-100 px-2.5 py-1 text-xs font-semibold text-sky-700">
                {{ spell.school?.name || 'Unknown School' }}
              </span>
              <span v-if="spell.ritual"
                class="inline-flex rounded-full bg-purple-100 px-2.5 py-1 text-xs font-semibold text-purple-700">
                Ritual
              </span>
              <span v-if="spell.concentration"
                class="inline-flex rounded-full bg-amber-100 px-2.5 py-1 text-xs font-semibold text-amber-700">
                Concentration
              </span>
            </div>
          </div>

          <!-- Main content -->
          <div class="sm:px-6 grid grid-cols-3 gap-4 py-4 md:grid-cols-3">
            <!-- Left column -->
            <div class="space-y-6">
              <div>
                <h4 class="text-sm font-medium text-gray-500">Casting Time</h4>
                <p class="mt-1 text-sm text-gray-900">{{ spell.castingTime }}</p>
              </div>

              <div>
                <h4 class="text-sm font-medium text-gray-500">Range</h4>
                <p class="mt-1 text-sm text-gray-900">{{ spell.range }}</p>
              </div>

              <div>
                <h4 class="text-sm font-medium text-gray-500">Components</h4>
                <p class="mt-1 text-sm text-gray-900">
                  {{ spell.components?.$values?.join(', ') || 'None' }}
                  <span v-if="spell.material" class="block mt-1 text-xs text-gray-500 italic">
                    {{ spell.material }}
                  </span>
                </p>
              </div>

              <div>
                <h4 class="text-sm font-medium text-gray-500">Duration</h4>
                <p class="mt-1 text-sm text-gray-900">{{ spell.duration }}</p>
              </div>

              <div>
                <h4 class="text-sm font-medium text-gray-500">Classes</h4>
                <p class="mt-1 text-sm text-gray-900">{{ classNames }}</p>
              </div>

              <div v-if="spell.attackType">
                <h4 class="text-sm font-medium text-gray-500">Attack Type</h4>
                <p class="mt-1 text-sm text-gray-900 capitalize">{{ spell.attackType }}</p>
              </div>
            </div>

            <!-- Right column - Description -->
            <div class="space-y-6 col-span-2">
              <div>
                <h4 class="text-sm font-medium text-gray-500">Description</h4>

                <div class="mt-1 text-sm text-gray-900 space-y-4 prose prose-sm max-w-none">
                  <template v-if="spell?.desc?.$values">
                    <template v-for="(paragraph, index) in processContent(spell.desc.$values)" :key="index">
                      <!-- Table content -->
                      <div v-if="paragraph.type === 'table'" class="overflow-x-auto mb-4">
                        <table class="min-w-full divide-y divide-gray-200 border border-gray-200">
                          <thead class="bg-gray-50">
                            <tr>
                              <th v-for="(header, headerIndex) in parseTable(paragraph.content).headers"
                                :key="headerIndex"
                                class="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-b">
                                {{ header }}
                              </th>
                            </tr>
                          </thead>
                          <tbody class="bg-white divide-y divide-gray-200">
                            <tr v-for="(row, rowIndex) in parseTable(paragraph.content).rows" :key="rowIndex">
                              <td v-for="(cell, cellIndex) in row" :key="cellIndex"
                                class="px-3 py-2 whitespace-nowrap text-sm text-gray-500 border-r last:border-r-0">
                                {{ cell }}
                              </td>
                            </tr>
                          </tbody>
                        </table>
                      </div>

                      <!-- Regular text content -->
                      <div v-else>
                        <!-- For paragraphs with headers -->
                        <div v-if="paragraph.content.startsWith('#')">
                          <h5 class="font-bold text-base mt-4">{{ paragraph.content.replace(/^#+\s+/, '') }}</h5>
                        </div>
                        <!-- Normal paragraphs -->
                        <p v-else class="whitespace-pre-line">{{ paragraph.content }}</p>
                      </div>
                    </template>
                  </template>
                </div>
              </div>

              <div v-if="spell.higherLevel?.$values?.length">
                <h4 class="text-sm font-medium text-gray-500">At Higher Levels</h4>
                <div class="mt-1 text-sm text-gray-900 space-y-2">
                  <p v-for="(paragraph, index) in spell.higherLevel.$values" :key="index">
                    {{ paragraph }}
                  </p>
                </div>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div class="sm:p-6 sticky bottom-0 bg-white z-10 border-t border-gray-200 px-4 py-4">
            <div class="flex justify-end">
              <button type="button" @click="closeModal"
                class="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-700">
                Close
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Add custom table styles */
:deep(table) {
  border-collapse: collapse;
  width: 100%;
  margin: 1rem 0;
}

:deep(th),
:deep(td) {
  padding: 0.5rem;
  border: 1px solid #e5e7eb;
  text-align: left;
}

:deep(th) {
  background-color: #f9fafb;
  font-weight: 600;
}

/* Make sure pre-formatted text doesn't break layout */
:deep(pre) {
  white-space: pre-wrap;
  overflow-x: auto;
}

/* Add some spacing for headers in the content */
:deep(h1),
:deep(h2),
:deep(h3),
:deep(h4),
:deep(h5),
:deep(h6) {
  margin-top: 1rem;
  margin-bottom: 0.5rem;
  font-weight: 600;
}
</style>