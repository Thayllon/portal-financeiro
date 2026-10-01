import { describe, it, expect } from 'vitest';
import { diasEmAberto, duracaoDias, formatarDuracao } from './processo.utils';

describe('processo.utils', () => {
  it('diasEmAberto conta dias corridos desde o cadastro', () => {
    const dezDiasAtras = new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString();
    expect(diasEmAberto(dezDiasAtras)).toBe(10);
  });

  it('diasEmAberto usa o encerramento quando informado', () => {
    expect(diasEmAberto('2026-01-01T00:00:00Z', '2026-01-06T00:00:00Z')).toBe(5);
  });

  it('duracaoDias retorna null sem data de início', () => {
    expect(duracaoDias(undefined)).toBeNull();
  });

  it('duracaoDias mede início até conclusão', () => {
    expect(duracaoDias('2026-02-01T00:00:00Z', '2026-02-04T00:00:00Z')).toBe(3);
  });

  it('formatarDuracao cobre os casos', () => {
    expect(formatarDuracao(null)).toBe('—');
    expect(formatarDuracao(0)).toBe('menos de 1 dia');
    expect(formatarDuracao(1)).toBe('1 dia');
    expect(formatarDuracao(7)).toBe('7 dias');
  });
});
