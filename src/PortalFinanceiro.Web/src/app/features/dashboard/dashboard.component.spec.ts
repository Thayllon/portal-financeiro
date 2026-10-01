import { calcularYtdAcumulado, montarPrevisaoMensal } from './dashboard.component';
import { Dashboard, MensalResumoAnual } from '../../core/models/dashboard.model';

function mes(m: number, recebido: number, pago: number): MensalResumoAnual {
  return {
    mes: m,
    totalReceitas: recebido,
    totalRecebido: recebido,
    totalDespesas: pago,
    totalPago: pago,
    saldo: recebido - pago,
    saldoRealizado: recebido - pago,
    saldoAcumulado: 0,
    saldoRealizadoAcumulado: 0
  };
}

describe('calcularYtdAcumulado', () => {
  const meses = [
    mes(1, 0, 0),
    mes(2, 0, 0),
    mes(3, 0, 0),
    mes(4, 0, 0),
    mes(5, 0, 0),
    mes(6, 0, 0),
    mes(7, 0, 0),
    mes(8, 1000, 0),
    mes(9, 1000, 0),
    mes(10, 0, 0),
    mes(11, 0, 0),
    mes(12, 0, 0)
  ];

  it('soma recebido menos pago de janeiro até o mês selecionado', () => {
    const r = calcularYtdAcumulado(meses, 9);
    expect(r.recebido).toBe(2000);
    expect(r.pago).toBe(0);
    expect(r.saldo).toBe(2000);
  });

  it('antes dos lançamentos o acumulado é zero', () => {
    const r = calcularYtdAcumulado(meses, 7);
    expect(r.saldo).toBe(0);
  });

  it('não considera meses após o selecionado', () => {
    const r = calcularYtdAcumulado(meses, 8);
    expect(r.recebido).toBe(1000);
  });

  it('saldo negativo quando pagou mais do que recebeu', () => {
    const r = calcularYtdAcumulado([mes(1, 500, 800), mes(2, 100, 100)], 2);
    expect(r.saldo).toBe(-300);
  });
});

describe('montarPrevisaoMensal', () => {
  const d = {
    totalReceitasPrevisto: 1500,
    totalDespesasPrevisto: 900
  } as Dashboard;

  it('deriva receitas, despesas e saldo previstos', () => {
    const r = montarPrevisaoMensal(d);
    expect(r.receitas).toBe(1500);
    expect(r.despesas).toBe(900);
    expect(r.saldo).toBe(600);
  });

  it('saldo previsto negativo quando despesas superam receitas', () => {
    const r = montarPrevisaoMensal({ totalReceitasPrevisto: 300, totalDespesasPrevisto: 800 } as Dashboard);
    expect(r.saldo).toBe(-500);
  });
});