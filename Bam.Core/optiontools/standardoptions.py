def StandardOptions(parser):
  parser.add_option("-f", "--force", dest="force", action="store_true", default=False, help="Force writing")
  parser.add_option("-u", "--updateheader", dest="updateheader", action="store_true", default=False, help="Update headers")
  parser.add_option("-i", "--ignoreheaders", dest="ignoreheader", action="store_true", default=False, help="Ignore header updates")
  (options,args) = parser.parse_args()

  extra_args = []
  if options.force:
    extra_args.append("-f")
  if options.updateheader:
    extra_args.append("-uh")
  if options.ignoreheader:
    extra_args.append("-ih")

  return options, args, extra_args
