import { Component, inject } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';
import { PrivacidadeService } from '../../core/services/privacidade.service';

@Component({
  selector: 'app-privacidade-toggle',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './privacidade-toggle.component.html',
  styleUrl: './privacidade-toggle.component.scss'
})
export class PrivacidadeToggleComponent {
  protected privacidade = inject(PrivacidadeService);
}
