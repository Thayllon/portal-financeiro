import { Directive, ElementRef, HostListener, forwardRef, inject } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Directive({
  selector: '[currencyInput]',
  standalone: true,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CurrencyInputDirective),
      multi: true,
    },
  ],
})
export class CurrencyInputDirective implements ControlValueAccessor {
  private el = inject(ElementRef<HTMLInputElement>);
  private onChange: (value: number) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: number): void {
    if (value === null || value === undefined || isNaN(value)) {
      this.el.nativeElement.value = '';
      return;
    }
    this.el.nativeElement.value = this.formatCurrency(value);
  }

  registerOnChange(fn: (value: number) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  @HostListener('input', ['$event'])
  onInput(_event: Event): void {
    const raw = this.el.nativeElement.value;
    const numeric = this.parseCurrency(raw);

    this.el.nativeElement.value = this.formatCurrency(numeric);
    this.onChange(numeric);
  }

  @HostListener('focus')
  onFocus(): void {
    setTimeout(() => this.el.nativeElement.select());
  }

  @HostListener('blur')
  onBlur(): void {
    this.onTouched();
  }

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    const allowed = ['Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight', 'Home', 'End'];
    const isCtrlA = event.ctrlKey && event.key === 'a';
    const isCtrlC = event.ctrlKey && event.key === 'c';
    const isCtrlV = event.ctrlKey && event.key === 'v';
    const isCtrlX = event.ctrlKey && event.key === 'x';

    if (allowed.includes(event.key) || isCtrlA || isCtrlC || isCtrlV || isCtrlX) {
      return;
    }

    if (event.key === ',' || event.key === '.') {
      const input = this.el.nativeElement;
      const hasDecimal = input.value.includes(',');
      if (hasDecimal) {
        event.preventDefault();
      }
      return;
    }

    if (!/^\d$/.test(event.key)) {
      event.preventDefault();
    }
  }

  private formatCurrency(value: number): string {
    if (value === 0) return '';
    return value.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });
  }

  private parseCurrency(value: string): number {
    if (!value) return 0;

    let cleaned = value.replace(/[^\d,.-]/g, '');

    const lastComma = cleaned.lastIndexOf(',');
    const lastDot = cleaned.lastIndexOf('.');

    if (lastComma > lastDot) {
      cleaned = cleaned.replace(/\./g, '').replace(',', '.');
    } else {
      cleaned = cleaned.replace(/,/g, '');
    }

    const num = parseFloat(cleaned);
    return isNaN(num) ? 0 : Math.round(num * 100) / 100;
  }
}
