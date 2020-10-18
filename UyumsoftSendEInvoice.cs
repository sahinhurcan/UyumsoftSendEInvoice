public class UyumsoftSendEInvoice : ICommandSendUBL {

        #region Props & Fields

        private uyumsoft.IntegrationClient _serviceProxy;

        public InvoiceType Invoice { get; set; }
        public ServiceInfo ServiceInfo { get; set; }
        public Task<ServiceResponse> Result { get; set; }

        IMapper _mapper { get; set; }

        #endregion

        public UyumsoftSendEInvoice(InvoiceType invoiceType, IMapper mapper) {
            this.Invoice = invoiceType;
            this._mapper = mapper;
        }
        
        public async Task Execute() {
            if (Invoice == null) { throw new ArgumentNullException("Invoice boş olamaz!"); }

            this._serviceProxy = UyumsoftHelper.CreateServiceProxy(ServiceInfo.ServiceUrl, ServiceInfo.UserName, ServiceInfo.Password);

            var uyumsoftInvoiceType = this._mapper.Map<uyumsoft.InvoiceType>(Invoice);

            uyumsoft.InvoiceInfo[] invoices = new[]
            {
                new uyumsoft.InvoiceInfo()
                {
                    Invoice = uyumsoftInvoiceType,
                    LocalDocumentId = "localBelgeAydisi",
                }
            };

            this.Result = _serviceProxy?.SaveAsDraftAsync(invoices)
                .ContinueWith(d => {
                    var result = new ServiceResponse() {
                        Hatali = false,
                    };

                    if (d.IsCanceled || d.IsFaulted) {
                        result.Hatali = true;
                        foreach (System.Exception innerException in d.Exception.Flatten().InnerExceptions) {
                            result.Istisna += innerException.ToString();
                        }
                    } else {
                        result.Sonuc = d.IsCompletedSuccessfully ? "İşlem başarıyla tamamlandı" : "İşlem tamamlandı!";
                        result.Data = new { d.Result };
                    }

                    return result;
                });

        }

        public void Dispose() {
            (this._serviceProxy as ICommunicationObject)?.Close();
        }
    }