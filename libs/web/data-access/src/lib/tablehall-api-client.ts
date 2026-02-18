import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { paths, components } from '@tablehall/api-contracts';

export type ApiSchemas = components['schemas'];

@Injectable({ providedIn: 'root' })
export class TableHallApiClient {
  private readonly http = inject(HttpClient);

  // Placeholder: tu typeras au fur et à mesure que les routes existent dans OpenAPI
  // Exemple futur: getCampaign(id: string) { ... }
}
