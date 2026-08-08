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
    const caret = this.el.nativeElement.selectionStart ?? raw.length;
    const digitsBefore = (raw.slice(0, caret).match(/\d/g) ?? []).length;

    const cleaned = this.sanitize(raw);
    this.el.nativeElement.value = cleaned;
    this.setCaret(digitsBefore, cleaned);
    this.onChange(this.parseCurrency(cleaned));
  }

  @HostListener('focus')
  onFocus(): void {
    setTimeout(() => this.el.nativeElement.select());
  }

  @HostListener('blur')
  onBlur(): void {
    const value = this.parseCurrency(this.el.nativeElement.value);
    this.el.nativeElement.value = this.formatCurrency(value);
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
      return;
    }

    if (!/^\d$/.test(event.key)) {
      event.preventDefault();
    }
  }

  private sanitize(text: string): string {
    const normalized = text.includes(',') ? text : text.replace(/\./g, ',');
    let out = '';
    let seenComma = false;
    let decimals = 0;
    for (const ch of normalized) {
      if (ch === ',') {
        if (!seenComma) {
          out += ch;
          seenComma = true;
        }
        continue;
      }
      if (ch >= '0' && ch <= '9') {
        if (seenComma && decimals >= 2) continue;
        out += ch;
        if (seenComma) decimals++;
      }
    }
    return out;
  }

  private setCaret(digitsBefore: number, text: string) {
    let index = 0;
    let seen = 0;
    while (index < text.length && seen < digitsBefore) {
      if (/\d/.test(text[index])) seen++;
      index++;
    }
    this.el.nativeElement.setSelectionRange(index, index);
  }

  private formatCurrency(value: number): string {
    if (value === 0) return '';
    return value.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });
  }

  private parseCurrency(value: string): number {
    const t = (value || '').trim();
    if (!t) return 0;

    const normalized = t.replace(/\./g, '').replace(/,/g, '.');
    const num = parseFloat(normalized);
    return isNaN(num) ? 0 : Math.round(num * 100) / 100;
  }
}