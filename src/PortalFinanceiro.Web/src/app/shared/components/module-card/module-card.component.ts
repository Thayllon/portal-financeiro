import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';
import { ModuloPermissao } from '../../../core/models/permissao.model';

export interface ModuleCardItem {
  title: string;
  description: string;
  icon: string;
  route: string;
  modulo?: ModuloPermissao;
}

@Component({
  selector: 'app-module-card',
  standalone: true,
  imports: [RouterLink, LucideDynamicIcon],
  templateUrl: './module-card.component.html',
  styleUrl: './module-card.component.scss'
})
export class ModuleCardComponent {
  item = input.required<ModuleCardItem>();
}
