import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { motion, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { formatCurrency, formatDate } from '@/lib/utils';
import { Check, CreditCard, Sparkles, Zap, Crown, Building2, Receipt } from 'lucide-react';

const plans = [
  {
    name: 'Free',
    price: 0,
    icon: Zap,
    features: ['Up to 5 users', 'Basic dashboard', 'Email support'],
  },
  {
    name: 'Starter',
    price: 29,
    icon: Sparkles,
    features: ['Up to 25 users', 'Full dashboard', 'Priority email support', 'API access'],
  },
  {
    name: 'Professional',
    price: 99,
    icon: Crown,
    features: [
      'Up to 100 users',
      'Advanced analytics',
      'Phone support',
      'API access',
      'Custom branding',
    ],
    popular: true,
  },
  {
    name: 'Enterprise',
    price: 299,
    icon: Building2,
    features: [
      'Unlimited users',
      'Advanced analytics',
      'Dedicated support',
      'API access',
      'Custom branding',
      'SSO',
      'SLA',
    ],
  },
];

export function BillingPage() {
  const { data: subscription, isLoading: subLoading } = useQuery({
    queryKey: ['subscription'],
    queryFn: () => api.getSubscription(),
  });

  const { data: invoicesData, isLoading: invLoading } = useQuery({
    queryKey: ['invoices'],
    queryFn: () => api.getInvoices(),
  });

  return (
    <div>
      <PageHeader
        title="Billing"
        description="Manage your subscription and billing"
      />

      <StaggerContainer className="space-y-8">
        {/* Current Plan */}
        <StaggerItem>
          <motion.div
            whileHover={{ scale: 1.005 }}
            transition={{ duration: 0.2 }}
            className="glass rounded-xl p-6"
          >
            <div className="flex items-center gap-3 mb-4">
              <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
                <CreditCard className="h-5 w-5 text-primary" />
              </div>
              <div>
                <h3 className="font-semibold">Current Plan</h3>
                <p className="text-sm text-muted-foreground">Your current subscription details</p>
              </div>
            </div>

            {subLoading ? (
              <div className="flex items-center justify-between">
                <div className="space-y-2">
                  <Skeleton className="h-8 w-32" />
                  <Skeleton className="h-4 w-24" />
                </div>
                <Skeleton className="h-6 w-20 rounded-full" />
              </div>
            ) : (
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-3xl font-bold gradient-text capitalize">
                    {subscription?.tier || 'Free'}
                  </p>
                  <p className="text-muted-foreground">
                    {subscription?.monthlyPrice
                      ? `${formatCurrency(subscription.monthlyPrice)}/month`
                      : 'Free forever'}
                  </p>
                  {subscription?.currentPeriodEnd && (
                    <p className="text-sm text-muted-foreground mt-1">
                      Renews on {formatDate(subscription.currentPeriodEnd)}
                    </p>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  <span
                    className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-medium ${
                      subscription?.status === 'active'
                        ? 'bg-green-500/10 border border-green-500/20 text-green-400'
                        : 'bg-yellow-500/10 border border-yellow-500/20 text-yellow-400'
                    }`}
                  >
                    <span className={`w-1.5 h-1.5 rounded-full mr-1.5 ${
                      subscription?.status === 'active' ? 'bg-green-400' : 'bg-yellow-400'
                    }`} />
                    {subscription?.status || 'Active'}
                  </span>
                </div>
              </div>
            )}
          </motion.div>
        </StaggerItem>

        {/* Available Plans */}
        <StaggerItem>
          <div>
            <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <Sparkles className="h-5 w-5 text-primary" />
              Available Plans
            </h2>
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
              {plans.map((plan, index) => {
                const Icon = plan.icon;
                const isCurrent = subscription?.tier?.toLowerCase() === plan.name.toLowerCase();

                return (
                  <motion.div
                    key={plan.name}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.1 * index }}
                    whileHover={{ y: -4, scale: 1.02 }}
                    className={`glass rounded-xl overflow-hidden relative ${
                      plan.popular ? 'ring-2 ring-primary' : ''
                    }`}
                  >
                    {plan.popular && (
                      <div className="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-primary to-accent" />
                    )}

                    <div className="p-5">
                      {plan.popular && (
                        <span className="inline-flex items-center gap-1 text-xs font-medium text-primary mb-2">
                          <Crown className="h-3 w-3" />
                          Most Popular
                        </span>
                      )}

                      <div className="flex items-center gap-2 mb-3">
                        <div className={`p-2 rounded-lg ${
                          plan.popular
                            ? 'bg-gradient-to-br from-primary/20 to-accent/20'
                            : 'bg-muted/50'
                        }`}>
                          <Icon className={`h-4 w-4 ${plan.popular ? 'text-primary' : 'text-muted-foreground'}`} />
                        </div>
                        <h3 className="font-semibold">{plan.name}</h3>
                      </div>

                      <div className="mb-4">
                        <span className="text-3xl font-bold">
                          {formatCurrency(plan.price)}
                        </span>
                        {plan.price > 0 && (
                          <span className="text-muted-foreground text-sm">/month</span>
                        )}
                      </div>

                      <ul className="space-y-2 mb-5">
                        {plan.features.map((feature) => (
                          <li key={feature} className="flex items-center text-sm">
                            <Check className="h-4 w-4 text-primary mr-2 flex-shrink-0" />
                            <span className="text-muted-foreground">{feature}</span>
                          </li>
                        ))}
                      </ul>

                      <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                        <Button
                          className={`w-full ${
                            isCurrent
                              ? 'bg-muted text-muted-foreground'
                              : plan.popular
                              ? 'bg-gradient-to-r from-primary to-accent hover:opacity-90'
                              : 'bg-secondary hover:bg-secondary/80'
                          }`}
                          disabled={isCurrent}
                        >
                          {isCurrent ? 'Current Plan' : 'Upgrade'}
                        </Button>
                      </motion.div>
                    </div>
                  </motion.div>
                );
              })}
            </div>
          </div>
        </StaggerItem>

        {/* Invoice History */}
        <StaggerItem>
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.5 }}
            className="glass rounded-xl overflow-hidden"
          >
            <div className="p-6 border-b border-border/30">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-accent/10">
                  <Receipt className="h-5 w-5 text-accent" />
                </div>
                <div>
                  <h3 className="font-semibold">Invoice History</h3>
                  <p className="text-sm text-muted-foreground">Your billing history</p>
                </div>
              </div>
            </div>

            <div className="p-6">
              {invLoading ? (
                <div className="space-y-4">
                  {[...Array(3)].map((_, i) => (
                    <div key={i} className="flex items-center justify-between">
                      <Skeleton className="h-4 w-24" />
                      <Skeleton className="h-4 w-20" />
                      <Skeleton className="h-4 w-16" />
                      <Skeleton className="h-6 w-14 rounded-full" />
                    </div>
                  ))}
                </div>
              ) : invoicesData?.invoices.length === 0 ? (
                <div className="text-center py-8">
                  <Receipt className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
                  <p className="text-muted-foreground">No invoices yet</p>
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-border/50">
                        <th className="text-left py-3 font-medium text-muted-foreground text-sm">Invoice</th>
                        <th className="text-left py-3 font-medium text-muted-foreground text-sm">Date</th>
                        <th className="text-left py-3 font-medium text-muted-foreground text-sm">Amount</th>
                        <th className="text-left py-3 font-medium text-muted-foreground text-sm">Status</th>
                      </tr>
                    </thead>
                    <tbody>
                      {invoicesData?.invoices.map((invoice, index) => (
                        <motion.tr
                          key={invoice.id}
                          initial={{ opacity: 0, x: -10 }}
                          animate={{ opacity: 1, x: 0 }}
                          transition={{ delay: 0.05 * index }}
                          className="border-b border-border/30 last:border-0 hover:bg-white/5 transition-colors"
                        >
                          <td className="py-4 font-medium">{invoice.invoiceNumber}</td>
                          <td className="py-4 text-muted-foreground">
                            {formatDate(invoice.issuedAt)}
                          </td>
                          <td className="py-4 font-medium">{formatCurrency(invoice.amount)}</td>
                          <td className="py-4">
                            <span
                              className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ${
                                invoice.status === 'paid'
                                  ? 'bg-green-500/10 border border-green-500/20 text-green-400'
                                  : 'bg-yellow-500/10 border border-yellow-500/20 text-yellow-400'
                              }`}
                            >
                              <span className={`w-1.5 h-1.5 rounded-full mr-1.5 ${
                                invoice.status === 'paid' ? 'bg-green-400' : 'bg-yellow-400'
                              }`} />
                              {invoice.status}
                            </span>
                          </td>
                        </motion.tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </motion.div>
        </StaggerItem>
      </StaggerContainer>
    </div>
  );
}
