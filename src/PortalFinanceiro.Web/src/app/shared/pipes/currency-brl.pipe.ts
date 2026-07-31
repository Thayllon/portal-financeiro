import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'currencyBRL', standalone: true })
export class CurrencyBRLPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value === null || value === undefined) return 'R$ 0,00';
    return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }
}
