import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { LancamentoListagemComponent } from './lancamento-listagem.component';
import { AuthService } from '../../core/services/auth.service';
import { AuthRepository } from '../../core/repositories/auth.repository';
import { ReceitaRepository, DespesaRepository } from '../../core/repositories/lancamento.repository';
import { CategoriaReceitaRepository, CategoriaDespesaRepository, CategoriaServicoRepository } from '../../core/repositories/categoria.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';
import { ParceriaRepository } from '../../core/repositories/parceria.repository';
import { ContratoRepository } from '../../core/repositories/contrato.repository';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { Router } from '@angular/router';
import { ModuloPermissao } from '../../core/models/permissao.model';

const STORAGE_KEY = 'portal-financeiro.auth';

function semearSessao(permissoes: { modulo: ModuloPermissao; nivel: number }[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({
    usuarioId: 'usuario-teste',
    nome: 'teste',
    email: 'teste@portal.com',
    isAdmin: false,
    token: 'token-teste',
    permissoes
  }));
}

async function criarComponente(tipo: 'receita' | 'despesa') {
  TestBed.configureTestingModule({
    imports: [LancamentoListagemComponent],
    providers: [
      AuthService,
      { provide: AuthRepository, useValue: {} },
      { provide: Router, useValue: { navigate: () => Promise.resolve(true) } },
      { provide: ReceitaRepository, useValue: { listar: () => of([]) } },
      { provide: DespesaRepository, useValue: { listar: () => of([]) } },
      { provide: CategoriaReceitaRepository, useValue: { listar: () => of([]) } },
      { provide: CategoriaDespesaRepository, useValue: { listar: () => of([]) } },
      { provide: CategoriaServicoRepository, useValue: { listar: () => of([]) } },
      { provide: ContaBancariaRepository, useValue: { listar: () => of([]) } },
      { provide: PessoaRepository, useValue: { listar: () => of([]) } },
      { provide: ParceriaRepository, useValue: { listar: () => of([]) } },
      { provide: ContratoRepository, useValue: { listar: () => of([]) } },
      { provide: NotificationService, useValue: { error: () => undefined, success: () => undefined } },
      { provide: ConfirmService, useValue: { confirm: () => Promise.resolve(false) } }
    ]
  });
  TestBed.overrideComponent(LancamentoListagemComponent, { set: { template: '' } });
  await TestBed.compileComponents();
  const fixture = TestBed.createComponent(LancamentoListagemComponent);
  fixture.componentRef.setInput('tipo', tipo);
  return fixture.componentInstance;
}

describe('LancamentoListagemComponent.podeEscrever', () => {
  afterEach(() => {
    localStorage.removeItem(STORAGE_KEY);
    TestBed.resetTestingModule();
  });

  it('receita com escrita em receitas libera inclusao', async () => {
    semearSessao([{ modulo: 'receitas', nivel: 2 }]);
    const comp = await criarComponente('receita');
    expect(comp.podeEscrever()).toBe(true);
  });

  it('receita com leitura em receitas bloqueia inclusao', async () => {
    semearSessao([{ modulo: 'receitas', nivel: 1 }]);
    const comp = await criarComponente('receita');
    expect(comp.podeEscrever()).toBe(false);
  });

  it('despesa com escrita em despesas libera inclusao', async () => {
    semearSessao([{ modulo: 'despesas', nivel: 2 }]);
    const comp = await criarComponente('despesa');
    expect(comp.podeEscrever()).toBe(true);
  });

  it('despesa sem permissao bloqueia inclusao', async () => {
    semearSessao([]);
    const comp = await criarComponente('despesa');
    expect(comp.podeEscrever()).toBe(false);
  });

  it('receita nao herda escrita de despesas', async () => {
    semearSessao([{ modulo: 'despesas', nivel: 2 }]);
    const comp = await criarComponente('receita');
    expect(comp.podeEscrever()).toBe(false);
  });
});
