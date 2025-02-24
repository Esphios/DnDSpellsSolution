import { createRouter, createWebHistory } from 'vue-router';
import SpellGrid from '@/components/SpellGrid.vue';

const routes = [
  { path: '/', redirect: '/spells' }, 
  { path: '/spells', component: SpellGrid },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
