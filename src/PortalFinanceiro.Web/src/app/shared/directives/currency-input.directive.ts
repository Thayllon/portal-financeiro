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
  private focused = false;

  writeValue(value: number): void {
    if (value === null || value === undefined || isNaN(value)) value = 0;
    if (this.focused) return;
    this.el.nativeElement.value = value === 0 ? '' : this.toDisplay(value);
  }

  registerOnChange(fn: (value: number) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  @HostListener('input')
  onInput(): void {
    const raw = this.el.nativeElement.value;
    const { digits, decs, hasSep } = this.parseParts(raw);
    const cleaned = this.toRaw(digits, decs, hasSep);
    this.el.nativeElement.value = cleaned;
    this.el.nativeElement.setSelectionRange(cleaned.length, cleaned.length);

    this.onChange(this.toValue(digits, decs));
  }

  @HostListener('focus')
  onFocus(): void {
    this.focused = true;
    this.el.nativeElement.select();
  }

  @HostListener('blur')
  onBlur(): void {
    this.focused = false;
    const { digits, decs } = this.parseParts(this.el.nativeElement.value);
    const value = this.toValue(digits, decs);
    this.el.nativeElement.value = value === 0 ? '' : this.toDisplay(value);
    this.onTouched();
  }

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    const allowed = ['Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight', 'Home', 'End'];
    const isCtrl = event.ctrlKey || event.metaKey;
    const isCtrlKey = ['a', 'c', 'v', 'x'].includes(event.key.toLowerCase());

    if (allowed.includes(event.key) || (isCtrl && isCtrlKey)) return;
    if (event.key === ',' || event.key === '.' || /^\d$/.test(event.key)) return;
    event.preventDefault();
  }

  /** Extrai dígitos e quantos deles são decimais (0-2), usando o ÚLTIMO separador (`,` ou `.`) como vírgula decimal. */
  private parseParts(text: string): { digits: string; decs: number; hasSep: boolean } {
    const t = (text || '').replace(/\s+/g, '');
    if (!t) return { digits: '', decs: 0, hasSep: false };

    const lastSep = Math.max(t.lastIndexOf(','), t.lastIndexOf('.'));
    if (lastSep < 0) {
      return { digits: t.replace(/\D/g, ''), decs: 0, hasSep: false };
    }

    const intDigits = t.slice(0, lastSep).replace(/\D/g, '');
    const decRaw = t.slice(lastSep + 1).replace(/\D/g, '');
    const decs = Math.min(decRaw.length, 2);
    return { digits: intDigits + decRaw, decs, hasSep: true };
  }

  private toRaw(digits: string, decs: number, hasSep: boolean): string {
    if (!digits) return hasSep ? ',' : '';
    const int = digits.slice(0, digits.length - decs) || '0';
    const dec = (decs > 0 || hasSep) ? ',' + digits.slice(digits.length - decs) : '';
    return int + dec;
  }

  private toValue(digits: string, decs: number): number {
    if (!digits) return 0;
    const int = digits.slice(0, digits.length - decs) || '0';
    const dec = decs > 0 ? digits.slice(digits.length - decs) : '';
    return parseFloat(int + '.' + dec);
  }

  private toDisplay(value: number): string {
    if (value === 0) return '';
    return value.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });
  }
}