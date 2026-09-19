interface StatusBadgeProps { status: string }

const STATUS_MAP: Record<string, string> = {
  Pending: 'badge-warning',
  Processing: 'badge-brand',
  Processed: 'badge-success',
  Failed: 'bg-red-100 text-red-700 badge',
};

const STATUS_LABEL: Record<string, string> = {
  Pending: 'Aguardando',
  Processing: 'Processando...',
  Processed: 'Pronto',
  Failed: 'Falhou',
};

export function StatusBadge({ status }: StatusBadgeProps) {
  const cls = STATUS_MAP[status] ?? 'badge-neutral';
  return <span className={cls}>{STATUS_LABEL[status] ?? status}</span>;
}