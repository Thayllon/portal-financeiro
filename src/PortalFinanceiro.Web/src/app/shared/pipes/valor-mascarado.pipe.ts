import { Pipe, PipeTransform, inject } from '@angular/core';
import { PrivacidadeService } from '../../core/services/privacidade.service';
import { CurrencyBRLPipe } from './currency-brl.pipe';

@Pipe({ name: 'valorMascarado', standalone: true, pure: false })
export class ValorMascaradoPipe implements PipeTransform {
  private privacidade = inject(PrivacidadeService);
  private currencyBRL = new CurrencyBRLPipe();

  transform(value: number | null | undefined): string {
    if (this.privacidade.valoresOcultos()) return '••••••';
    return this.currencyBRL.transform(value);
  }
}
